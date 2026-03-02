using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.SavedJobs.DTO;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.Repository;

public class SavedJobRepository : ISavedJobRepository
{
    private readonly FakeTopcvContext _context;

    public SavedJobRepository(FakeTopcvContext context)
    {
        _context = context;
    }

    public async Task<bool> IsCandidateRoleAsync(Guid userId)
    {
        // Kiểm tra xem User có tồn tại và Role có đúng là 2 (Candidate) không
        return await _context.Users.AnyAsync(u => u.Id == userId && u.Role == 2);
    }

    public async Task<Guid?> GetCandidateProfileIdByUserIdAsync(Guid userId)
    {
        var profile = await _context.CandidateProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        return profile?.Id;
    }

    public async Task<Guid> CreateEmptyProfileAsync(Guid userId)
    {
        // Tự động sinh ra 1 bản ghi Hồ sơ ứng viên trống
        var newProfile = new CandidateProfile
        {
            UserId = userId
            // Nếu bảng CandidateProfiles của bạn bắt buộc nhập FullName, Email... thì gán giá trị mặc định vào đây.
            // Ví dụ: FullName = "User mới", Email = "chuacapnhat@email.com"
        };

        _context.CandidateProfiles.Add(newProfile);
        await _context.SaveChangesAsync();

        return newProfile.Id;
    }

    public async Task<IEnumerable<SavedJobItemDto>> GetSavedJobsAsync(Guid candidateProfileId)
    {
        return await _context.SavedJobs
            .AsNoTracking()
            .Where(x => x.CandidateId == candidateProfileId)
            .OrderByDescending(x => x.SavedAt)
            .Select(x => new SavedJobItemDto
            {
                JobId = x.JobId,
                Title = x.Job.Title,
                Location = x.Job.Location,
                SalaryMin = x.Job.SalaryMin,
                SalaryMax = x.Job.SalaryMax,
                SavedAt = x.SavedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<Guid>> GetSavedJobIdsAsync(Guid candidateProfileId)
    {
        return await _context.SavedJobs
            .AsNoTracking()
            .Where(x => x.CandidateId == candidateProfileId)
            .Select(x => x.JobId)
            .ToListAsync();
    }

    public async Task<SavedJob?> GetSavedJobAsync(Guid candidateProfileId, Guid jobId)
    {
        return await _context.SavedJobs
            .FirstOrDefaultAsync(x => x.CandidateId == candidateProfileId && x.JobId == jobId);
    }

    public async Task<bool> CheckJobExistsAsync(Guid jobId)
    {
        return await _context.JobPosts.AnyAsync(x => x.Id == jobId);
    }

    public async Task AddAsync(SavedJob savedJob)
    {
        _context.SavedJobs.Add(savedJob);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(SavedJob savedJob)
    {
        _context.SavedJobs.Remove(savedJob);
        await _context.SaveChangesAsync();
    }
}