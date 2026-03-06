using RecruitmentInterviewManagementSystem.Models.DTOs;

namespace RecruitmentInterviewManagementSystem.Repositories
{
    public interface IViewListJobApplyRepository
    {
        Task<IEnumerable<ViewListJobApplyDTO>> GetAppliedJobsByCandidateId(Guid candidateId);
        Task<bool> DeleteApplication(Guid applicationId);
    }
}