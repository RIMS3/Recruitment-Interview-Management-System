    using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Models;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;

namespace RecruitmentInterviewManagementSystem.Infastructure.Repository
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly FakeTopcvContext _context;

        public ApplicationRepository(FakeTopcvContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckApplicationExistsAsync(Guid jobId, Guid candidateId)
        {
            return await _context.Applications
                .AnyAsync(a => a.JobId == jobId && a.CandidateId == candidateId);
        }

        public async Task CreateApplicationAsync(Application application)
        {
            await _context.Applications.AddAsync(application);
        }
        public async Task<Application?> GetApplicationByIdAsync(Guid applicationId)
        {
            return await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == applicationId);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}