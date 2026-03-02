using RecruitmentInterviewManagementSystem.Applications.Features.SavedJobs.DTO;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;

public interface ISavedJobRepository
{
    Task<IEnumerable<SavedJobItemDto>> GetSavedJobsAsync(Guid candidateProfileId);
    Task<IEnumerable<Guid>> GetSavedJobIdsAsync(Guid candidateProfileId);
    Task<SavedJob?> GetSavedJobAsync(Guid candidateProfileId, Guid jobId);
    Task<bool> CheckJobExistsAsync(Guid jobId);
    Task AddAsync(SavedJob savedJob);
    Task DeleteAsync(SavedJob savedJob);

    // 3 hàm mới phục vụ logic tự động tạo Profile
    Task<bool> IsCandidateRoleAsync(Guid userId);
    Task<Guid?> GetCandidateProfileIdByUserIdAsync(Guid userId);
    Task<Guid> CreateEmptyProfileAsync(Guid userId);
}