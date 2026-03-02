namespace RecruitmentInterviewManagementSystem.Applications.Features.SavedJobs.DTO;

public class SavedJobItemDto
{
    public Guid JobId { get; set; }
    public string? Title { get; set; }
    public string? Location { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public DateTime? SavedAt { get; set; }
}