namespace RecruitmentInterviewManagementSystem.Applications.Features.Cvs.DTO
{
    public class CvSummaryDto
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public int? ExperienceYears { get; set; }
        public bool IsDefault { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? TemplateId { get; set; }
    }

    public class CvDetailDto : CvSummaryDto
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateOnly? Birthday { get; set; }
        public int? Gender { get; set; }
        public string? Nationality { get; set; }
        public string? EducationSummary { get; set; }
        public string? Field { get; set; }
        public decimal? CurrentSalary { get; set; }
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public string? MimeType { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateCvRequest
    {
        public Guid CandidateId { get; set; }
        public string FullName { get; set; } = string.Empty;
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
        public bool IsDefault { get; set; }
        public IFormFile? File { get; set; }
        public string? TemplateId { get; set; }
    }

    public class UpdateCvRequest
    {
        public string FullName { get; set; } = string.Empty;
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
        public bool IsDefault { get; set; }
        public IFormFile? File { get; set; }
        public string? TemplateId { get; set; }
    }
}
