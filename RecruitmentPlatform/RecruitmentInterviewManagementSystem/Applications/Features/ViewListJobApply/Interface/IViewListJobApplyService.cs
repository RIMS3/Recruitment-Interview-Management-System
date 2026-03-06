using RecruitmentInterviewManagementSystem.Models.DTOs;

public interface IViewListJobApplyService
{
    Task<IEnumerable<ViewListJobApplyDTO>> GetListAppliedJobs(Guid candidateId);
    Task<bool> UnapplyJob(Guid applicationId);
}