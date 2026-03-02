using RecruitmentInterviewManagementSystem.Applications.Features.SavedJobs;
using RecruitmentInterviewManagementSystem.Applications.Features.SavedJobs.DTO;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement;

public class SavedJobService : ISavedJobService
{
    private readonly ISavedJobRepository _repository;

    public SavedJobService(ISavedJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SavedJobItemDto>> GetSavedJobsAsync(Guid candidateId)
    {
        return await _repository.GetSavedJobsAsync(candidateId);
    }

    public async Task<IEnumerable<Guid>> GetSavedJobIdsAsync(Guid candidateId)
    {
        return await _repository.GetSavedJobIdsAsync(candidateId);
    }

    public async Task<(bool IsSuccess, string Message, object? Data)> SaveJobAsync(SaveJobRequest request)
    {
        var validationError = await ValidateRequestAsync(request);
        if (validationError != null) return (false, validationError, null);

        var existingJob = await _repository.GetSavedJobAsync(request.CandidateId, request.JobId);
        if (existingJob != null)
        {
            return (true, "Job đã được lưu trước đó.", new { saved = true });
        }

        var newSavedJob = new SavedJob
        {
            CandidateId = request.CandidateId,
            JobId = request.JobId,
            SavedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(newSavedJob);
        return (true, "Lưu job thành công.", new { saved = true });
    }

    public async Task<(bool IsSuccess, string Message)> UnsaveJobAsync(SaveJobRequest request)
    {
        if (request.CandidateId == Guid.Empty || request.JobId == Guid.Empty)
            return (false, "CandidateId và JobId là bắt buộc.");

        var savedJob = await _repository.GetSavedJobAsync(request.CandidateId, request.JobId);
        if (savedJob == null)
        {
            return (false, "Không tìm thấy job đã lưu.");
        }

        await _repository.DeleteAsync(savedJob);
        return (true, "Đã bỏ lưu job.");
    }

    public async Task<(bool IsSuccess, string Message, bool IsSaved)> ToggleSavedJobAsync(SaveJobRequest request)
    {
        var validationError = await ValidateRequestAsync(request);
        if (validationError != null) return (false, validationError, false);

        var savedJob = await _repository.GetSavedJobAsync(request.CandidateId, request.JobId);

        if (savedJob == null)
        {
            await _repository.AddAsync(new SavedJob
            {
                CandidateId = request.CandidateId,
                JobId = request.JobId,
                SavedAt = DateTime.UtcNow
            });
            return (true, "Lưu job thành công.", true);
        }

        await _repository.DeleteAsync(savedJob);
        return (true, "Đã bỏ lưu job.", false);
    }

    // Hàm validate nội bộ
    private async Task<string?> ValidateRequestAsync(SaveJobRequest request)
    {
        if (request.CandidateId == Guid.Empty || request.JobId == Guid.Empty)
            return "CandidateId và JobId là bắt buộc.";

        if (!await _repository.CheckCandidateExistsAsync(request.CandidateId))
            return "Candidate không tồn tại.";

        if (!await _repository.CheckJobExistsAsync(request.JobId))
            return "Job không tồn tại.";

        return null; // Null nghĩa là hợp lệ
    }
}