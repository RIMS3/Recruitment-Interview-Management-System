using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Domain.InterfacesRepository
{
    public interface IApplicationRepository
    {
        Task<bool> CheckApplicationExistsAsync(Guid jobId, Guid candidateId);
        Task CreateApplicationAsync(Application application);
        Task<Application?> GetApplicationByIdAsync(Guid applicationId);

        Task SaveChangesAsync();
    }
}