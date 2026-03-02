using RecruitmentInterviewManagementSystem.Applications.Features.SavedJobs.DTO;
using RecruitmentInterviewManagementSystem.Models; // Hoặc namespace chứa entity SavedJob của bạn

namespace RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;

public interface ISavedJobRepository
{
    Task<IEnumerable<SavedJobItemDto>> GetSavedJobsAsync(Guid candidateId);
    Task<IEnumerable<Guid>> GetSavedJobIdsAsync(Guid candidateId);
    Task<SavedJob?> GetSavedJobAsync(Guid candidateId, Guid jobId);
    Task<bool> CheckCandidateExistsAsync(Guid candidateId);
    Task<bool> CheckJobExistsAsync(Guid jobId);
    Task AddAsync(SavedJob savedJob);
    Task DeleteAsync(SavedJob savedJob);
}