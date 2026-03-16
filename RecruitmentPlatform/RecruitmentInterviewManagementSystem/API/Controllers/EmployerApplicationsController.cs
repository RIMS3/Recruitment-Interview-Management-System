using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Application.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Domain.Enums;
using RecruitmentInterviewManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationDTO>>> GetApplications(
            [FromQuery] int? status,
            [FromQuery] string? searchTitle)
        {
            // 1. Lấy EmployerId từ Token (Đây là Id trong bảng EmployerProfiles)
            var employerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(employerIdClaim))
            {
                return Unauthorized(new { message = "Không xác định được danh tính nhà tuyển dụng." });
            }

            Guid currentEmployerId = Guid.Parse(employerIdClaim);

            // 2. Tìm CompanyId mà Employer này thuộc về (Dựa trên ảnh DB bảng EmployerProfiles)
            var employerProfile = await _context.EmployerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(ep => ep.Id == currentEmployerId);

            if (employerProfile == null)
            {
                return Ok(new List<ApplicationDTO>());
            }

            // 3. Truy vấn ứng viên dựa trên CompanyId của bài đăng (JobPost)
            var query = _context.Applications
                .AsNoTracking()
                .Include(a => a.Candidate).ThenInclude(c => c.User)
                .Include(a => a.Job)
                .Include(a => a.Cv)
                .Where(a => a.Job.CompanyId == employerProfile.CompanyId) // Lọc theo công ty
                .AsQueryable();

            // Lọc trạng thái
            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(searchTitle))
            {
                var keyword = searchTitle.Trim().ToLower();
                query = query.Where(a =>
                    a.Job.Title.ToLower().Contains(keyword) ||
                    (a.Candidate.User.FullName.ToLower().Contains(keyword))
                );
            }

            // 4. Map sang DTO
            var applications = await query
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new ApplicationDTO
                {
                    ApplicationId = a.Id,
                    CandidateName = a.Candidate.User.FullName,
                    CandidateEmail = a.Candidate.User.Email,
                    CandidateAvatar = a.Candidate.AvatarUrl,
                    JobTitle = a.Job.Title,
                    AppliedAt = a.AppliedAt ?? DateTime.Now,
                    Status = a.Status ?? 0,
                    CvUrl = a.Cv != null ? a.Cv.FileUrl : string.Empty
                })
                .ToListAsync();

            // 5. Cập nhật URL ảnh từ MinIO
            foreach (var app in applications)
            {
                if (!string.IsNullOrEmpty(app.CandidateAvatar))
                {
                    try { app.CandidateAvatar = await _minioService.GetUrlImage("avatars", app.CandidateAvatar); }
                    catch { app.CandidateAvatar = null; }
                }
            }

            return Ok(applications);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDTO request)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null) return NotFound();
            application.Status = (int)request.NewStatus;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{applicationId:guid}/cv")]
        public async Task<IActionResult> GetCvByApplication(Guid applicationId)
        {
            var application = await _context.Applications.Include(a => a.Cv).FirstOrDefaultAsync(a => a.Id == applicationId);
            if (application == null || application.Cv == null) return NotFound();
            return Ok(new { cvId = application.Cvid });
        }

        [HttpGet("{id}/avatar")]
        public async Task<IActionResult> GetAvatarUrl(Guid id)
        {
            var profile = await _context.CandidateProfiles.FindAsync(id);
            if (profile == null) return NotFound();
            string url = string.IsNullOrEmpty(profile.AvatarUrl) ? null : await _minioService.GetUrlImage("avatars", profile.AvatarUrl);
            return Ok(new { candidateId = id, avatarUrl = url });
        }
    }
}