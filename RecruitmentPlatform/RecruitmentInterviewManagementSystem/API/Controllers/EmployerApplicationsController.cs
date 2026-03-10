using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Application.DTO;
using RecruitmentInterviewManagementSystem.Domain.Enums;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/employer-applications")]
    public class EmployerApplicationsController : ControllerBase
    {
        private readonly FakeTopcvContext _context;

        public EmployerApplicationsController(FakeTopcvContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: Applications (All + Filter Backend)
        // =====================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationDTO>>> GetApplications(
    [FromQuery] Guid? companyId,
    [FromQuery] int? status,
    [FromQuery] string? searchTitle) // searchTitle bây giờ đóng vai trò là từ khóa chung
        {
            var query = _context.Applications
                .AsNoTracking()
                .Include(a => a.Candidate).ThenInclude(c => c.User)
                .Include(a => a.Job)
                .Include(a => a.Cv)
                .AsQueryable();

            if (companyId.HasValue) query = query.Where(a => a.Job.CompanyId == companyId.Value);

            if (status.HasValue) query = query.Where(a => a.Status == status.Value);

            // CẬP NHẬT Ở ĐÂY: Tìm theo cả Job Title HOẶC FullName của ứng viên
            if (!string.IsNullOrWhiteSpace(searchTitle))
            {
                var keyword = searchTitle.Trim().ToLower();
                query = query.Where(a =>
                    a.Job.Title.ToLower().Contains(keyword) ||
                    (a.Candidate != null && a.Candidate.User != null && a.Candidate.User.FullName.ToLower().Contains(keyword))
                );
            }

            var applications = await query
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new ApplicationDTO
                {
                    ApplicationId = a.Id,
                    CandidateName = a.Candidate.User.FullName,
                    CandidateEmail = a.Candidate.User.Email,
                    CandidateAvatar = a.Candidate.AvatarUrl,
                    JobTitle = a.Job.Title,
                    AppliedAt = a.AppliedAt ?? default,
                    Status = a.Status ?? 0,
                    CvUrl = a.Cv != null ? a.Cv.FileUrl : string.Empty
                })
                .ToListAsync();

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
    }
}