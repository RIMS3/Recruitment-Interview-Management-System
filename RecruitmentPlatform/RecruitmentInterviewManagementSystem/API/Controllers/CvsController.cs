using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio.DataModel.Args;
using RecruitmentInterviewManagementSystem.Applications.Features.Cvs.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Cvs.Interface;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Models;
using System.Security.Claims; // Thêm thư viện này để đọc Token

namespace RecruitmentInterviewManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CvsController : ControllerBase
{
    private readonly ICvService _cvService;
    private readonly IMinIOCV _minIO;
    private readonly FakeTopcvContext _context; // Bổ sung DbContext để chọc vào Database

    // Cập nhật Constructor để Inject FakeTopcvContext
    public CvsController(ICvService cvService, IMinIOCV minIOCV, FakeTopcvContext context)
    {
        _cvService = cvService;
        _minIO = minIOCV;
        _context = context;
    }

    // =======================================================
    // API MỚI: LẤY CANDIDATE ID TỪ TOKEN
    // =======================================================
    [HttpGet("my-candidate-id")]
    [Authorize] // Bắt buộc phải có token truyền lên
    public async Task<IActionResult> GetMyCandidateId()
    {
        // 1. Lấy UserId từ Token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                         ?? User.FindFirst("id")
                         ?? User.FindFirst("sub");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            return Unauthorized("Không tìm thấy thông tin xác thực UserId hợp lệ trong Token.");
        }

        // 2. Tìm ID Ứng viên tương ứng trong DB
        var candidate = await _context.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == userId);

        if (candidate == null)
        {
            return NotFound("Không tìm thấy hồ sơ ứng viên cho tài khoản này. Vui lòng cập nhật vai trò!");
        }

        // 3. Trả về JSON chứa candidateId
        return Ok(new { candidateId = candidate.Id });
    }
    // =======================================================

    [HttpGet("candidate/{candidateId:guid}")]
    public async Task<ActionResult<IEnumerable<CvSummaryDto>>> GetByCandidate(Guid candidateId)
    {
        var cvs = await _cvService.GetCvsByCandidateAsync(candidateId);
        return Ok(cvs);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CvDetailDto>> GetById(Guid id)
    {
        var cv = await _cvService.GetCvByIdAsync(id);
        if (cv == null) return NotFound("Không tìm thấy CV.");
        return Ok(cv);
    }

    [HttpPost]
    public async Task<ActionResult<CvDetailDto>> Create([FromForm] CreateCvRequest request)
    {
        try
        {
            var created = await _cvService.CreateCvAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CvDetailDto>> Update(Guid id, [FromForm] UpdateCvRequest request)
    {
        try
        {
            var updated = await _cvService.UpdateCvAsync(id, request);
            if (updated == null) return NotFound("Không tìm thấy CV.");
            return Ok(updated);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _cvService.DeleteCvAsync(id);
        if (!isDeleted) return NotFound("Không tìm thấy CV.");
        return Ok(new { message = "Xóa CV thành công." });
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> DownloadCv(Guid id)
    {
        try
        {
            var fileData = await _cvService.DownloadCvAsync(id);
            if (fileData == null) return NotFound("Không tìm thấy file CV.");

            return File(fileData.Value.Stream, fileData.Value.ContentType, fileData.Value.FileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi khi tải file: {ex.Message}");
        }
    }

    [HttpGet("images")]
    public async Task<IActionResult> GetImageCV()
    {
        var template1 = await _minIO.GetUrlImage("avatars", "template1.png");
        var template2 = await _minIO.GetUrlImage("avatars", "template2.png");
        var template3 = await _minIO.GetUrlImage("avatars", "template3.png");

        var images = new List<string>
        {
            template1,
            template2,
            template3
        };

        return Ok(images);
    }

    [HttpPut("{id:guid}/avatar")]
    //[Authorize] // Nhớ mở Comment cái này nếu bắt buộc user login mới được đổi ảnh
    public async Task<IActionResult> UploadAvatar(Guid id, IFormFile file)
    {
        try
        {
            var result = await _cvService.UpdateAvatarAsync(id, file);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            // Có thể dùng GlobalExceptions (folder bạn có sẵn) để bắt lỗi này thay vì try-catch ở đây
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }
}