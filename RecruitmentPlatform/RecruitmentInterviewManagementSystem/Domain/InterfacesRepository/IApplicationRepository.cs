using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Domain.InterfacesRepository
{
    public interface IApplicationRepository
    {
        Task<Application?> GetApplicationByIdAsync(Guid applicationId);

    }
}