using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Cvs.DTO;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.API.Controllers;

[Route("api/cvs/{cvId:guid}/editor")]
[ApiController]
public class CvsEditorController : ControllerBase
{
    private readonly FakeTopcvContext _db;

    public CvsEditorController(FakeTopcvContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<CvEditorDataDto>> GetEditorData(Guid cvId)
    {
        var cv = await _db.Cvs.FirstOrDefaultAsync(x => x.Id == cvId);
        if (cv == null) return NotFound("Không tìm thấy CV.");

        return Ok(await BuildEditorData(cv));
    }

    [HttpPut]
    public async Task<ActionResult<CvEditorDataDto>> UpdateEditorData(Guid cvId, [FromBody] UpdateCvEditorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest("Họ tên không được để trống.");

        var cv = await _db.Cvs.FirstOrDefaultAsync(x => x.Id == cvId);
        if (cv == null) return NotFound("Không tìm thấy CV.");

        cv.FullName = request.FullName;
        cv.Position = request.Position;
        cv.EducationSummary = request.Summary;
        cv.UpdatedAt = DateTime.UtcNow;

        var oldEducations = await _db.CvEducations.Where(x => x.Cvid == cvId).ToListAsync();
        var oldExperiences = await _db.CvExperiences.Where(x => x.Cvid == cvId).ToListAsync();
        var oldProjects = await _db.CvProjects.Where(x => x.Cvid == cvId).ToListAsync();
        var oldCertificates = await _db.CvCertificates.Where(x => x.Cvid == cvId).ToListAsync();
        var oldSkills = await _db.CvSkills.Where(x => x.Cvid == cvId).ToListAsync();

        _db.CvEducations.RemoveRange(oldEducations);
        _db.CvExperiences.RemoveRange(oldExperiences);
        _db.CvProjects.RemoveRange(oldProjects);
        _db.CvCertificates.RemoveRange(oldCertificates);
        _db.CvSkills.RemoveRange(oldSkills);

        var educations = request.Educations.Select(x => new CvEducation
        {
            Id = x.Id ?? Guid.NewGuid(),
            Cvid = cvId,
            SchoolName = x.SchoolName,
            Major = x.Major,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Description = x.Description
        });

        var experiences = request.Experiences.Select(x => new CvExperience
        {
            Id = x.Id ?? Guid.NewGuid(),
            Cvid = cvId,
            CompanyName = x.CompanyName,
            Position = x.Position,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Description = x.Description
        });

        var projects = request.Projects.Select(x => new CvProject
        {
            Id = x.Id ?? Guid.NewGuid(),
            Cvid = cvId,
            ProjectName = x.ProjectName,
            Role = x.Role,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Description = x.Description
        });

        var certificates = request.Certificates.Select(x => new CvCertificate
        {
            Id = x.Id ?? Guid.NewGuid(),
            Cvid = cvId,
            CertificateName = x.CertificateName,
            Organization = x.Organization,
            IssueDate = x.IssueDate,
            ExpiredDate = x.ExpiredDate
        });

        var skills = request.Skills
            .Where(x => !string.IsNullOrWhiteSpace(x.SkillName))
            .Select(x => new CvSkill
            {
                Cvid = cvId,
                SkillName = x.SkillName.Trim(),
                Level = x.Level
            });

        await _db.CvEducations.AddRangeAsync(educations);
        await _db.CvExperiences.AddRangeAsync(experiences);
        await _db.CvProjects.AddRangeAsync(projects);
        await _db.CvCertificates.AddRangeAsync(certificates);
        await _db.CvSkills.AddRangeAsync(skills);

        await _db.SaveChangesAsync();

        return Ok(await BuildEditorData(cv));
    }

    private async Task<CvEditorDataDto> BuildEditorData(Cv cv)
    {
        var educations = await _db.CvEducations.Where(x => x.Cvid == cv.Id)
            .Select(x => new CvEducationItemDto
            {
                Id = x.Id,
                SchoolName = x.SchoolName,
                Major = x.Major,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Description = x.Description
            }).ToListAsync();

        var experiences = await _db.CvExperiences.Where(x => x.Cvid == cv.Id)
            .Select(x => new CvExperienceItemDto
            {
                Id = x.Id,
                CompanyName = x.CompanyName,
                Position = x.Position,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Description = x.Description
            }).ToListAsync();

        var projects = await _db.CvProjects.Where(x => x.Cvid == cv.Id)
            .Select(x => new CvProjectItemDto
            {
                Id = x.Id,
                ProjectName = x.ProjectName,
                Role = x.Role,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Description = x.Description
            }).ToListAsync();

        var certificates = await _db.CvCertificates.Where(x => x.Cvid == cv.Id)
            .Select(x => new CvCertificateItemDto
            {
                Id = x.Id,
                CertificateName = x.CertificateName,
                Organization = x.Organization,
                IssueDate = x.IssueDate,
                ExpiredDate = x.ExpiredDate
            }).ToListAsync();

        var skills = await _db.CvSkills.Where(x => x.Cvid == cv.Id)
            .Select(x => new CvSkillItemDto
            {
                SkillName = x.SkillName,
                Level = x.Level
            }).ToListAsync();

        return new CvEditorDataDto
        {
            CvId = cv.Id,
            CandidateId = cv.CandidateId,
            FullName = cv.FullName,
            Position = cv.Position,
            Summary = cv.EducationSummary,
            Educations = educations,
            Experiences = experiences,
            Projects = projects,
            Certificates = certificates,
            Skills = skills
        };
    }
}