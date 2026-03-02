namespace RecruitmentInterviewManagementSystem.Applications.Features.SavedJobs.DTO;

public class SaveJobRequest
{
    public Guid CandidateId { get; set; }
    public Guid JobId { get; set; }
}