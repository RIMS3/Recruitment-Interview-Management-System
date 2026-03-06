using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Models;
using RecruitmentInterviewManagementSystem.Models.DTOs;

namespace RecruitmentInterviewManagementSystem.Repositories
{
    public class ViewListJobApplyRepository : IViewListJobApplyRepository
    {
        private readonly FakeTopcvContext _context; // Thay bằng tên DbContext của bạn

        public ViewListJobApplyRepository(FakeTopcvContext context) => _context = context;

        public async Task<IEnumerable<ViewListJobApplyDTO>> GetAppliedJobsByCandidateId(Guid candidateId)
        {
            return await (from app in _context.Applications
                          join job in _context.JobPosts on app.JobId equals job.Id
                          join comp in _context.Companies on job.CompanyId equals comp.Id
                          where app.CandidateId == candidateId
                          select new ViewListJobApplyDTO
                          {
                              ApplicationId = app.Id,
                              JobTitle = job.Title,
                              CompanyName = comp.Name,
                              AppliedAt = app.AppliedAt,
                              Status = app.Status
                          })

                          .OrderByDescending(a => a.AppliedAt)
                          .ToListAsync();
        }

        public async Task<bool> DeleteApplication(Guid applicationId)
        {
            var app = await _context.Applications.FindAsync(applicationId);
            if (app == null) return false;
            _context.Applications.Remove(app);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}