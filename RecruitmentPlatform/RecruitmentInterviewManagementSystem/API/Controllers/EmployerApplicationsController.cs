using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Application.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Domain.Enums;
using RecruitmentInterviewManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecruitmentInterviewManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/employer-applications")]
    public class EmployerApplicationsController : ControllerBase
    {
        private readonly FakeTopcvContext _context;
        private readonly IMinIOCV _minioService;

        public EmployerApplicationsController(FakeTopcvContext context, IMinIOCV minioService)
        {
            _context = context;
            _minioService = minioService;
        }

        // =====================================================
        // GET: Applications (All + Filter Backend)
        // =====================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationDTO>>> GetApplications(
            [FromQuery] Guid? companyId,
            [FromQuery] int? status,
            [FromQuery] string? searchTitle)
        {
            var query = _context.Applications
                .AsNoTracking()
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.User)
                .Include(a => a.Job)
                .Include(a => a.Cv)
                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(a => a.Job.CompanyId == companyId.Value);

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            // Tìm theo cả Job Title HOẶC FullName của ứng viên
            if (!string.IsNullOrWhiteSpace(searchTitle))
            {
                var keyword = searchTitle.Trim().ToLower();
                query = query.Where(a =>
                    a.Job.Title.ToLower().Contains(keyword) ||
                    (a.Candidate != null && a.Candidate.User != null && a.Candidate.User.FullName.ToLower().Contains(keyword))
                );
            }

            // 1. Truy vấn dữ liệu cơ bản từ Database
            var applications = await query
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new ApplicationDTO
                {
                    ApplicationId = a.Id,
                    CandidateName = a.Candidate.User.FullName,
                    CandidateEmail = a.Candidate.User.Email,
                    CandidateAvatar = a.Candidate.AvatarUrl, // Tạm thời lấy tên file ảnh
                    JobTitle = a.Job.Title,
                    AppliedAt = a.AppliedAt ?? default,
                    Status = a.Status ?? 0,
                    CvUrl = a.Cv != null ? a.Cv.FileUrl : string.Empty
                })
                .ToListAsync();

            // 2. Sinh link MinIO hàng loạt cho những ứng viên có ảnh
            foreach (var app in applications)
            {
                if (!string.IsNullOrEmpty(app.CandidateAvatar))
                {
                    try
                    {
                        // Đổi tên file thành link có thể truy cập được
                        app.CandidateAvatar = await _minioService.GetUrlImage("avatars", app.CandidateAvatar);
                    }
                    catch
                    {
                        // Bỏ qua lỗi (set null) nếu MinIO gặp sự cố với file này để không sập toàn bộ API
                        app.CandidateAvatar = null;
                    }
                }
            }

            // 3. Trả về danh sách đã hoàn thiện
            return Ok(applications);
        }

        // =====================================================
        // PATCH: Update Status
        // =====================================================
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDTO request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            if (!Enum.IsDefined(typeof(ApplicationStatus), request.NewStatus))
                return BadRequest("Invalid application status.");

            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
                return NotFound();

            var currentStatus = (ApplicationStatus)(application.Status ?? 0);

            if (currentStatus != ApplicationStatus.Pending)
                return BadRequest("Application has already been processed.");

            application.Status = (int)request.NewStatus;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =====================================================
        // GET CV BY APPLICATION
        // =====================================================
        [HttpGet("{applicationId:guid}/cv")]
        public async Task<IActionResult> GetCvByApplication(Guid applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.Cv)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                return NotFound("Application not found");

            if (application.Cv == null)
                return BadRequest("Candidate has not uploaded CV");

            return Ok(new
            {
                cvId = application.Cvid
            });
        }

        // =====================================================
        // GET AVATAR BY CANDIDATE ID (Lấy lẻ 1 người nếu cần)
        // =====================================================
        [HttpGet("{id}/avatar")]
        public async Task<IActionResult> GetAvatarUrl(Guid id)
        {
            var profile = await _context.CandidateProfiles.FindAsync(id);
            if (profile == null)
            {
                return NotFound(new { message = "Không tìm thấy hồ sơ." });
            }

            if (string.IsNullOrEmpty(profile.AvatarUrl))
            {
                return Ok(new
                {
                    candidateId = id,
                    avatarUrl = (string)null,
                    message = "Ứng viên chưa có ảnh đại diện."
                });
            }

            try
            {
                string displayUrl = await _minioService.GetUrlImage("avatars", profile.AvatarUrl);

                return Ok(new
                {
                    candidateId = id,
                    avatarUrl = displayUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
    }
}