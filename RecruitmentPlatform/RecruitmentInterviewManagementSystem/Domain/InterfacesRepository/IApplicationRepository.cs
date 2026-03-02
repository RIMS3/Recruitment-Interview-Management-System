using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Domain.InterfacesRepository
{
    public interface IApplicationRepository
    {
        Task<bool> IsAlreadyAppliedAsync(Guid jobId, Guid candidateId);
        Task AddAsync(Application application);
    }
}