namespace RecruitmentInterviewManagementSystem.Applications.Features.Cvs.DTO
{

    public class CvEducationItemDto
    {
        public Guid? Id { get; set; }
        public string? SchoolName { get; set; }
        public string? Major { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Description { get; set; }
    }

    public class CvExperienceItemDto
    {
        public Guid? Id { get; set; }
        public string? CompanyName { get; set; }
        public string? Position { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Description { get; set; }
    }

    public class CvProjectItemDto
    {
        public Guid? Id { get; set; }
        public string? ProjectName { get; set; }
        public string? Role { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Description { get; set; }
    }

    public class CvCertificateItemDto
    {
        public Guid? Id { get; set; }
        public string? CertificateName { get; set; }
        public string? Organization { get; set; }
        public DateOnly? IssueDate { get; set; }
        public DateOnly? ExpiredDate { get; set; }
    }

    public class CvSkillItemDto
    {
        public string SkillName { get; set; } = string.Empty;
        public int? Level { get; set; }
    }

    public class CvEditorDataDto
    {
        public Guid CvId { get; set; }
        public Guid CandidateId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Summary { get; set; }
        public List<CvEducationItemDto> Educations { get; set; } = new();
        public List<CvExperienceItemDto> Experiences { get; set; } = new();
        public List<CvProjectItemDto> Projects { get; set; } = new();
        public List<CvCertificateItemDto> Certificates { get; set; } = new();
        public List<CvSkillItemDto> Skills { get; set; } = new();
    }

    public class UpdateCvEditorRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Summary { get; set; }
        public List<CvEducationItemDto> Educations { get; set; } = new();
        public List<CvExperienceItemDto> Experiences { get; set; } = new();
        public List<CvProjectItemDto> Projects { get; set; } = new();
        public List<CvCertificateItemDto> Certificates { get; set; } = new();
        public List<CvSkillItemDto> Skills { get; set; } = new();
    }
}
