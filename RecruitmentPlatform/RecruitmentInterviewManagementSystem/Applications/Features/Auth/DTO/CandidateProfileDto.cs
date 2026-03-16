namespace RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO
{
    public class CandidateProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public int? Gender { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public int? ExperienceYears { get; set; }
        public decimal? CurrentSalary { get; set; }
        public decimal? DesiredSalary { get; set; }
        public string? JobLevel { get; set; }
        public string? Summary { get; set; }
        public bool IsCvPro { get; set; }
    }
}
