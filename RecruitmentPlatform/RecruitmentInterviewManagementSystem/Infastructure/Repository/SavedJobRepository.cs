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

    public async Task<IEnumerable<SavedJobItemDto>> GetSavedJobsAsync(Guid candidateId)
    {
        return await _context.SavedJobs
            .AsNoTracking()
            .Where(x => x.CandidateId == candidateId)
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

    public async Task<IEnumerable<Guid>> GetSavedJobIdsAsync(Guid candidateId)
    {
        return await _context.SavedJobs
            .AsNoTracking()
            .Where(x => x.CandidateId == candidateId)
            .Select(x => x.JobId)
            .ToListAsync();
    }

    public async Task<SavedJob?> GetSavedJobAsync(Guid candidateId, Guid jobId)
    {
        return await _context.SavedJobs
            .FirstOrDefaultAsync(x => x.CandidateId == candidateId && x.JobId == jobId);
    }

    public async Task<bool> CheckCandidateExistsAsync(Guid candidateId)
    {
        return await _context.CandidateProfiles.AnyAsync(x => x.Id == candidateId);
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