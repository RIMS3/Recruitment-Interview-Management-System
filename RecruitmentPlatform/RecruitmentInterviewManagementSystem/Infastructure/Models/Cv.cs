using System;
using System.Collections.Generic;

namespace RecruitmentInterviewManagementSystem.Models;

public partial class Cv
{
    public Guid Id { get; set; }

    public Guid CandidateId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public DateOnly? Birthday { get; set; }

    public int? Gender { get; set; }

    public string? Nationality { get; set; }

    public string? Position { get; set; }

    public int? ExperienceYears { get; set; }

    public string? EducationSummary { get; set; }

    public string? Field { get; set; }

    public decimal? CurrentSalary { get; set; }

    public string? FileName { get; set; }

    public string? FileUrl { get; set; }

    public string? MimeType { get; set; }

    public bool? IsDefault { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? TemplateId { get; set; }

    public bool IsDeleted { get; set; } = false;

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual CandidateProfile Candidate { get; set; } = null!;

    public virtual ICollection<CvCertificate> CvCertificates { get; set; } = new List<CvCertificate>();

    public virtual ICollection<CvEducation> CvEducations { get; set; } = new List<CvEducation>();

    public virtual ICollection<CvExperience> CvExperiences { get; set; } = new List<CvExperience>();

    public virtual ICollection<CvProject> CvProjects { get; set; } = new List<CvProject>();

    public virtual ICollection<CvSkill> CvSkills { get; set; } = new List<CvSkill>();
}
