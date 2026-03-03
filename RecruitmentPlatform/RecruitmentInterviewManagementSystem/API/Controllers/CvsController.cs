using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio.DataModel.Args;
using RecruitmentInterviewManagementSystem.Applications.Features.Cvs.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CvsController : ControllerBase
{
    private readonly FakeTopcvContext _db;
    private readonly IMinIOCV _minioService;
    private readonly IConfiguration _configuration;

    public CvsController(FakeTopcvContext db, IMinIOCV minioService, IConfiguration configuration)
    {
        _db = db;
        _minioService = minioService;
        _configuration = configuration;
    }

    [HttpGet("candidate/{candidateId:guid}")]
    public async Task<ActionResult<IEnumerable<CvSummaryDto>>> GetByCandidate(Guid candidateId)
    {
        var cvs = await _db.Cvs
            .Where(cv => cv.CandidateId == candidateId)
            .OrderByDescending(cv => cv.IsDefault)
            .ThenByDescending(cv => cv.UpdatedAt ?? cv.CreatedAt)
            .Select(cv => new CvSummaryDto
            {
                Id = cv.Id,
                CandidateId = cv.CandidateId,
                FullName = cv.FullName,
                Position = cv.Position,
                ExperienceYears = cv.ExperienceYears,
                IsDefault = cv.IsDefault ?? false,
                UpdatedAt = cv.UpdatedAt
            })
            .ToListAsync();

        return Ok(cvs);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CvDetailDto>> GetById(Guid id)
    {
        var cv = await _db.Cvs.FirstOrDefaultAsync(x => x.Id == id);
        if (cv == null) return NotFound("Không tìm thấy CV.");

        return Ok(await MapToDetailDto(cv));
    }

    [HttpPost]
    public async Task<ActionResult<CvDetailDto>> Create([FromForm] CreateCvRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest("Họ tên không được để trống.");

        var candidateExists = await _db.CandidateProfiles.AnyAsync(c => c.Id == request.CandidateId);
        if (!candidateExists)
            return NotFound("Không tìm thấy hồ sơ ứng viên.");

        var cv = new Cv
        {
            Id = Guid.NewGuid(),
            CandidateId = request.CandidateId,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Birthday = request.Birthday,
            Gender = request.Gender,
            Nationality = request.Nationality,
            Position = request.Position,
            ExperienceYears = request.ExperienceYears,
            EducationSummary = request.EducationSummary,
            Field = request.Field,
            CurrentSalary = request.CurrentSalary,
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (request.File != null && request.File.Length > 0)
        {
            // SỬA: Thêm tên bucket "cvs"
            var bucketName = _configuration["Minio:CvBucket"] ?? "cvs";
            var objectName = await _minioService.UploadAsync(request.File, bucketName);
            cv.FileName = request.File.FileName;
            cv.MimeType = request.File.ContentType;
            cv.FileUrl = objectName;
        }

        if (cv.IsDefault == true)
        {
            await ResetDefaultCvForCandidate(request.CandidateId);
        }

        _db.Cvs.Add(cv);
        await _db.SaveChangesAsync();

        var created = await MapToDetailDto(cv);
        return CreatedAtAction(nameof(GetById), new { id = cv.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CvDetailDto>> Update(Guid id, [FromForm] UpdateCvRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest("Họ tên không được để trống.");

        var cv = await _db.Cvs.FirstOrDefaultAsync(x => x.Id == id);
        if (cv == null) return NotFound("Không tìm thấy CV.");

        cv.FullName = request.FullName;
        cv.Email = request.Email;
        cv.PhoneNumber = request.PhoneNumber;
        cv.Address = request.Address;
        cv.Birthday = request.Birthday;
        cv.Gender = request.Gender;
        cv.Nationality = request.Nationality;
        cv.Position = request.Position;
        cv.ExperienceYears = request.ExperienceYears;
        cv.EducationSummary = request.EducationSummary;
        cv.Field = request.Field;
        cv.CurrentSalary = request.CurrentSalary;
        cv.IsDefault = request.IsDefault;
        cv.UpdatedAt = DateTime.UtcNow;

        if (request.File != null && request.File.Length > 0)
        {
            var bucketName = _configuration["Minio:CvBucket"] ?? "cvs";

            // SỬA: Thêm tên bucket "cvs" khi xóa file cũ
            if (!string.IsNullOrWhiteSpace(cv.FileUrl))
            {
                await _minioService.DeleteAsync(cv.FileUrl, bucketName);
            }

            // SỬA: Thêm tên bucket "cvs" khi upload file mới
            var objectName = await _minioService.UploadAsync(request.File, bucketName);
            cv.FileName = request.File.FileName;
            cv.MimeType = request.File.ContentType;
            cv.FileUrl = objectName;
        }

        if (cv.IsDefault == true)
        {
            await ResetDefaultCvForCandidate(cv.CandidateId, cv.Id);
        }

        await _db.SaveChangesAsync();

        return Ok(await MapToDetailDto(cv));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var cv = await _db.Cvs.FirstOrDefaultAsync(x => x.Id == id);
        if (cv == null) return NotFound("Không tìm thấy CV.");

        // SỬA: Thêm tên bucket "cvs" khi xóa file
        if (!string.IsNullOrWhiteSpace(cv.FileUrl))
        {
            var bucketName = _configuration["Minio:CvBucket"] ?? "cvs";
            await _minioService.DeleteAsync(cv.FileUrl, bucketName);
        }

        _db.Cvs.Remove(cv);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Xóa CV thành công." });
    }

    private async Task<CvDetailDto> MapToDetailDto(Cv cv)
    {
        // SỬA: Đổi tên biến fallback từ "avatars" thành "cvs" cho chuẩn logic
        var fileBucket = _configuration["Minio:CvBucket"] ?? "cvs";
        var displayFileUrl = string.IsNullOrWhiteSpace(cv.FileUrl)
            ? null
            : await _minioService.GetUrlImage(fileBucket, cv.FileUrl);

        return new CvDetailDto
        {
            Id = cv.Id,
            CandidateId = cv.CandidateId,
            FullName = cv.FullName,
            Email = cv.Email,
            PhoneNumber = cv.PhoneNumber,
            Address = cv.Address,
            Birthday = cv.Birthday,
            Gender = cv.Gender,
            Nationality = cv.Nationality,
            Position = cv.Position,
            ExperienceYears = cv.ExperienceYears,
            EducationSummary = cv.EducationSummary,
            Field = cv.Field,
            CurrentSalary = cv.CurrentSalary,
            FileName = cv.FileName,
            FileUrl = displayFileUrl,
            MimeType = cv.MimeType,
            IsDefault = cv.IsDefault ?? false,
            CreatedAt = cv.CreatedAt,
            UpdatedAt = cv.UpdatedAt
        };
    }

    private async Task ResetDefaultCvForCandidate(Guid candidateId, Guid? excludedCvId = null)
    {
        var others = await _db.Cvs
            .Where(x => x.CandidateId == candidateId && (!excludedCvId.HasValue || x.Id != excludedCvId.Value))
            .ToListAsync();

        foreach (var item in others)
        {
            item.IsDefault = false;
            item.UpdatedAt = DateTime.UtcNow;
        }
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> DownloadCv(Guid id)
    {
        // 1. Lấy thông tin CV từ Database
        var cv = await _db.Cvs.FirstOrDefaultAsync(x => x.Id == id);
        if (cv == null || string.IsNullOrEmpty(cv.FileUrl))
            return NotFound("Không tìm thấy file CV.");

        try
        {
            var bucketName = _configuration["Minio:CvBucket"] ?? "cvs";

            // 2. Lấy luồng dữ liệu (Stream) của file từ MinIO
            // Lưu ý: Bạn cần thêm method GetObjectStream vào IMinIOCV nếu chưa có
            var memoryStream = new MemoryStream();
            await _minioService.GetObjectStreamAsync(bucketName, cv.FileUrl, stream =>
            {
                stream.CopyTo(memoryStream);
            });

            memoryStream.Position = 0;

            // 3. Trả về file với ContentType và tên file gốc (cv.FileName)
            return File(memoryStream, cv.MimeType ?? "application/octet-stream", cv.FileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi khi tải file: {ex.Message}");
        }
    }
}