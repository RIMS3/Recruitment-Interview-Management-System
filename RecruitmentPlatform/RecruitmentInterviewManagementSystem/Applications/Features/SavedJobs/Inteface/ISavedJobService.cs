using RecruitmentInterviewManagementSystem.Applications.Features.SavedJobs.DTO;

namespace RecruitmentInterviewManagementSystem.Applications.Features.SavedJobs;

public interface ISavedJobService
{
    Task<IEnumerable<SavedJobItemDto>> GetSavedJobsAsync(Guid candidateId);
    Task<IEnumerable<Guid>> GetSavedJobIdsAsync(Guid candidateId);

    // Sử dụng Tuple hoặc custom response object để trả về kết quả kèm thông báo
    Task<(bool IsSuccess, string Message, object? Data)> SaveJobAsync(SaveJobRequest request);
    Task<(bool IsSuccess, string Message)> UnsaveJobAsync(SaveJobRequest request);
    Task<(bool IsSuccess, string Message, bool IsSaved)> ToggleSavedJobAsync(SaveJobRequest request);
}