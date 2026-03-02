using RecruitmentInterviewManagementSystem.Models;

public interface IApplicationRepository
{
    Task<bool> IsAlreadyAppliedAsync(Guid jobId, Guid candidateId);
    Task AddAsync(ApplicationEntity application);
}