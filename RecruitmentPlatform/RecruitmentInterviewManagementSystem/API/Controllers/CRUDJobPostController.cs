using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Models;
using RecruitmentInterviewManagementSystem.DTOs;

namespace RecruitmentInterviewManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CRUDJobPostController : ControllerBase
{
    private readonly FakeTopcvContext _context;

    public CRUDJobPostController(FakeTopcvContext context)
    {
        _context = context;
    }

    // CREATE JOB
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateJob(CRUDCreateJobPostRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized("UserId not found in token");
        }

        var userId = Guid.Parse(userIdClaim.Value);
        var employer = await _context.EmployerProfiles
            .FirstOrDefaultAsync(e => e.Id == userId);

        if (employer == null)
            return BadRequest("Employer profile not found");

        var job = new JobPost
        {
            Id = Guid.NewGuid(),
            CompanyId = employer.CompanyId,
            Title = request.Title,
            Description = request.Description,
            Requirement = request.Requirement,
            Benefit = request.Benefit,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            Location = request.Location,
            JobType = request.JobType,
            ExpireAt = request.ExpireAt,
            Experience = request.Experience,
            IsActive = true,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.JobPosts.Add(job);
        await _context.SaveChangesAsync();

        return Ok(job);
    }

    // READ ALL JOB OF COMPANY
    [HttpGet("my-jobs")]
    public async Task<IActionResult> GetMyJobs()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized("UserId not found in token");
        }

        var userId = Guid.Parse(userIdClaim.Value);

        var employer = await _context.EmployerProfiles
            .FirstOrDefaultAsync(e => e.Id == userId);

        var jobs = await _context.JobPosts
            .Where(j => j.CompanyId == employer.CompanyId)
            .ToListAsync();

        return Ok(jobs);
    }

    // UPDATE JOB
    [HttpPut("update")]
    public async Task<IActionResult> UpdateJob(CRUDUpdateJobPostRequest request)
    {
        var job = await _context.JobPosts
            .FirstOrDefaultAsync(j => j.Id == request.JobId);

        if (job == null)
            return NotFound();

        job.Title = request.Title;
        job.Description = request.Description;
        job.Requirement = request.Requirement;
        job.Benefit = request.Benefit;
        job.SalaryMin = request.SalaryMin;
        job.SalaryMax = request.SalaryMax;
        job.Location = request.Location;
        job.JobType = request.JobType;
        job.ExpireAt = request.ExpireAt;
        job.Experience = request.Experience;

        await _context.SaveChangesAsync();

        return Ok(job);
    }

    // DELETE JOB
    [HttpDelete("{jobId}")]
    public async Task<IActionResult> DeleteJob(Guid jobId)
    {
        var job = await _context.JobPosts
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job == null)
            return NotFound();

        _context.JobPosts.Remove(job);

        await _context.SaveChangesAsync();

        return Ok("Job deleted");
    }
}