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
        // candidateId từ Request gửi lên thực chất là UserId
        var profileId = await _repository.GetCandidateProfileIdByUserIdAsync(candidateId);
        if (profileId == null) return new List<SavedJobItemDto>();

        return await _repository.GetSavedJobsAsync(profileId.Value);
    }

    public async Task<IEnumerable<Guid>> GetSavedJobIdsAsync(Guid candidateId)
    {
        var profileId = await _repository.GetCandidateProfileIdByUserIdAsync(candidateId);
        if (profileId == null) return new List<Guid>();

        return await _repository.GetSavedJobIdsAsync(profileId.Value);
    }

    public async Task<(bool IsSuccess, string Message, object? Data)> SaveJobAsync(SaveJobRequest request)
    {
        var (errorMessage, profileId) = await ValidateAndGetOrCreateProfileAsync(request.CandidateId, request.JobId);
        if (errorMessage != null || profileId == null)
            return (false, errorMessage ?? "Lỗi không xác định", null);

        var existingJob = await _repository.GetSavedJobAsync(profileId.Value, request.JobId);
        if (existingJob != null) return (true, "Job đã được lưu trước đó.", new { saved = true });

        var newSavedJob = new SavedJob
        {
            CandidateId = profileId.Value,
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

        var profileId = await _repository.GetCandidateProfileIdByUserIdAsync(request.CandidateId);
        if (profileId == null) return (false, "Không tìm thấy hồ sơ ứng viên.");

        var savedJob = await _repository.GetSavedJobAsync(profileId.Value, request.JobId);
        if (savedJob == null) return (false, "Không tìm thấy job đã lưu.");

        await _repository.DeleteAsync(savedJob);
        return (true, "Đã bỏ lưu job.");
    }

    public async Task<(bool IsSuccess, string Message, bool IsSaved)> ToggleSavedJobAsync(SaveJobRequest request)
    {
        var (errorMessage, profileId) = await ValidateAndGetOrCreateProfileAsync(request.CandidateId, request.JobId);
        if (errorMessage != null || profileId == null)
            return (false, errorMessage ?? "Lỗi không xác định", false);

        var savedJob = await _repository.GetSavedJobAsync(profileId.Value, request.JobId);

        if (savedJob == null)
        {
            await _repository.AddAsync(new SavedJob
            {
                CandidateId = profileId.Value,
                JobId = request.JobId,
                SavedAt = DateTime.UtcNow
            });
            return (true, "Lưu job thành công.", true);
        }

        await _repository.DeleteAsync(savedJob);
        return (true, "Đã bỏ lưu job.", false);
    }

    // Hàm quan trọng nhất: Vừa Check Role, vừa Tự động tạo Profile nếu thiếu
    private async Task<(string? ErrorMessage, Guid? ProfileId)> ValidateAndGetOrCreateProfileAsync(Guid userId, Guid jobId)
    {
        if (userId == Guid.Empty || jobId == Guid.Empty)
            return ("CandidateId và JobId là bắt buộc.", null);

        // 1. Check Role == 2 (Candidate)
        if (!await _repository.IsCandidateRoleAsync(userId))
            return ("Tài khoản không tồn tại hoặc không phải là Ứng viên (Role = 2).", null);

        if (!await _repository.CheckJobExistsAsync(jobId))
            return ("Job không tồn tại.", null);

        // 2. Tra cứu ProfileId. Nếu ứng viên chưa có hồ sơ thì TỰ ĐỘNG TẠO!
        var profileId = await _repository.GetCandidateProfileIdByUserIdAsync(userId);
        if (profileId == null)
        {
            profileId = await _repository.CreateEmptyProfileAsync(userId);
        }

        return (null, profileId);
    }
}