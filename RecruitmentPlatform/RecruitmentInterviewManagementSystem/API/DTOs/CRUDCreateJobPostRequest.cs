namespace RecruitmentInterviewManagementSystem.DTOs;

public class CRUDCreateJobPostRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Requirement { get; set; }

    public string? Benefit { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public string? Location { get; set; }

    public int? JobType { get; set; }

    public DateTime? ExpireAt { get; set; }

    public int Experience { get; set; }
}