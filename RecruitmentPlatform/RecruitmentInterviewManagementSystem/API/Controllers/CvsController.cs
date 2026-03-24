using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio.DataModel.Args;
using RecruitmentInterviewManagementSystem.API.DTOs;
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
    [Authorize]
    public IActionResult GetMyCandidateId()
    {
        var profileId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(new { candidateId = profileId });
    }
    // =======================================================

    [HttpGet("candidate/{candidateId:guid}")]
    public async Task<ActionResult<CandidateCvOverviewDto>> GetByCandidate(Guid candidateId)
    {
        // 1. Thử tìm hồ sơ bằng cách coi candidateId là UserId (vì Frontend đang gửi UserId)
        var candidateProfile = await _context.CandidateProfiles
            .FirstOrDefaultAsync(c => c.UserId == candidateId);

        // 2. Nếu không tìm thấy, thử tìm coi candidateId là ProfileId (trường hợp gọi đúng ID)
        if (candidateProfile == null)
        {
            candidateProfile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(c => c.Id == candidateId);
        }

        // 3. Nếu cả 2 cách đều không thấy, thì mới trả về NotFound
        if (candidateProfile == null)
        {
            return NotFound("Không tìm thấy hồ sơ ứng viên tương ứng với tài khoản này.");
        }

        // 4. Lấy actualProfileId từ đối tượng tìm được để truyền vào Service
        var actualProfileId = candidateProfile.Id;

        // 5. Gọi Service với ID chuẩn
        var cvs = await _cvService.GetCvsByCandidateAsync(actualProfileId);
        var currentCvCount = cvs.Count();

        bool canCreateNew = candidateProfile.IsCvPro || currentCvCount < 2;

        var overview = new CandidateCvOverviewDto
        {
            IsCvPro = candidateProfile.IsCvPro,
            CurrentCvCount = currentCvCount,
            CanCreateNew = canCreateNew,
            Cvs = cvs
        };

        return Ok(overview);
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
        try
        {
            var isDeleted = await _cvService.DeleteCvAsync(id);

            // Trả về object JSON cho đồng bộ với phía React
            if (!isDeleted) return NotFound(new { message = "Không tìm thấy CV hoặc CV đã bị xóa." });

            return Ok(new { message = "Xóa CV thành công." });
        }
        catch (Exception ex)
        {
            // Lấy chi tiết lỗi sâu nhất (nếu có)
            var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

            // Trả về lỗi 500 KÈM THEO message chi tiết để biết tại sao lỗi
            return StatusCode(500, new { message = $"Lỗi server khi xóa CV: {errorMessage}" });
        }
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

    // get Service CVPro
    [HttpGet("cvpro/{cvid}")]
    public async Task<IActionResult> GetEditorData([FromRoute] Guid cvid)
    {
        try
        {
            var service = await _context.ServicePackages.FirstOrDefaultAsync(s => s.Id == cvid);

            return Ok(new CVProDTO
            {
                IdService = service.Id,
                Name = service.Name,
                Description = service.Description,
                Price = (decimal)service.Price
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi server: {ex.Message}");
        }
    }

    [HttpPost("import")]
    [Authorize]
    public async Task<IActionResult> ImportCv(IFormFile file)
    {
        try
        {
            // 1. Kiểm tra file tồn tại
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn file hợp lệ." });
            }

            // ==========================================
            // TẤM KHIÊN BẢO MẬT: CHẶN FILE ĐỘC HẠI
            // ==========================================

            // 1.1 Chặn dung lượng (Tối đa 5MB)
            var maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { message = "Dung lượng file quá lớn. Tối đa chỉ cho phép 5MB." });
            }

            // 1.2 Chặn định dạng đuôi file (Extension)
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Hệ thống chỉ chấp nhận định dạng PDF hoặc Word (.pdf, .doc, .docx)." });
            }

            // 1.3 Chặn Mime Type (Chống đổi đuôi file làm giả)
            var allowedMimeTypes = new[] {
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            };
            if (!allowedMimeTypes.Contains(file.ContentType))
            {
                return BadRequest(new { message = "Nội dung file không hợp lệ hoặc đã bị làm giả định dạng." });
            }

            // 1.4 Kiểm tra chữ ký file (File Signature)
            if (!IsValidFileSignature(file))
            {
                return BadRequest(new { message = "Phát hiện file bị đổi đuôi giả mạo! Chỉ chấp nhận file PDF/Word thật." });
            }

            // 2. Lấy thông tin User/Profile từ Token
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid extractedId))
            {
                return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            }

            // Tìm Candidate Profile (Logic 2 lớp để tránh lỗi NotFound)
            var candidateProfile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(c => c.UserId == extractedId);

            if (candidateProfile == null)
            {
                candidateProfile = await _context.CandidateProfiles
                    .FirstOrDefaultAsync(c => c.Id == extractedId);
            }

            if (candidateProfile == null)
            {
                return NotFound(new { message = "Không tìm thấy hồ sơ ứng viên của bạn." });
            }

            // 3. Đẩy file lên MinIO bằng hàm Service có sẵn
            string bucketName = "cvs";
            var objectName = await _minIO.UploadAsync(file, bucketName);

            // 4. Khởi tạo record CV và lưu vào SQL Database
            var newCv = new Cv
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateProfile.Id,
                FullName = "CV Import (" + file.FileName + ")", // Tên hiển thị ngoài UI

                FileName = file.FileName,
                MimeType = file.ContentType,

                // LƯU CỘT MỚI: Định dạng chuẩn "bucket/object"
                ImportedCvUrl = $"{bucketName}/{objectName}",

                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Cvs.Add(newCv);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Import CV thành công!",
                cvId = newCv.Id
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
        }
    }

    private bool IsValidFileSignature(IFormFile file)
    {
        // Danh sách chữ ký byte chuẩn của PDF và Word
        var signatures = new Dictionary<string, byte[]>
        {
            { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } }, // Bắt đầu bằng %PDF
            { ".docx", new byte[] { 0x50, 0x4B, 0x03, 0x04 } }, // Định dạng ZIP/OpenXML
            { ".doc", new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } // Định dạng Word cũ
        };

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!signatures.ContainsKey(ext)) return false;

        using (var stream = file.OpenReadStream())
        using (var reader = new BinaryReader(stream))
        {
            var expectedSignature = signatures[ext];
            var fileHeader = reader.ReadBytes(expectedSignature.Length);

            // Kiểm tra xem các byte đầu tiên của file có khớp với định dạng chuẩn không
            return fileHeader.SequenceEqual(expectedSignature);
        }
    }

    [HttpGet("{id:guid}/view-import")]
    [Authorize]
    public async Task<IActionResult> ViewImportedCv(Guid id)
    {
        try
        {
            // Tìm CV trong Database
            var cv = await _context.Cvs.FirstOrDefaultAsync(c => c.Id == id);

            if (cv == null || string.IsNullOrEmpty(cv.ImportedCvUrl))
            {
                return NotFound(new { message = "Không tìm thấy file CV gốc." });
            }

            // Tách tên bucket và tên object từ chuỗi "cvs/2026/03/xxx.pdf"
            var parts = cv.ImportedCvUrl.Split('/', 2);
            if (parts.Length != 2)
            {
                return BadRequest(new { message = "Đường dẫn file không hợp lệ." });
            }

            string bucketName = parts[0];
            string objectName = parts[1];

            // Dùng hàm có sẵn của ông để lấy Presigned URL từ MinIO
            var url = await _minIO.GetUrlImage(bucketName, objectName);

            // Trả link về cho React
            return Ok(new { url = url });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
        }
    }
}