using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infrastructure.Repository
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly FakeTopcvContext _context;

        public ApplicationRepository(FakeTopcvContext context)
        {
            _context = context;
        }

        public async Task<bool> IsAlreadyAppliedAsync(Guid jobId, Guid candidateId)
        {
            return await _context.Applications
                .AnyAsync(x => x.JobId == jobId && x.CandidateId == candidateId);
        }

        public async Task AddAsync(Application application)
        {
            await _context.Applications.AddAsync(application);
            await _context.SaveChangesAsync();
        }
    }
}