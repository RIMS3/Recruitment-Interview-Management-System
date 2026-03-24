using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Models;
using RecruitmentInterviewManagementSystem.Models.DTOs;
using RecruitmentInterviewManagementSystem.Domain.Enums;

namespace RecruitmentInterviewManagementSystem.Repositories
{
    public class ViewListJobApplyRepository : IViewListJobApplyRepository
    {
        private readonly FakeTopcvContext _context; 

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
                              JobId = job.Id,
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

            // KIỂM TRA: Nếu không tìm thấy hoặc trạng thái khác Pending (0) thì không cho phép xóa
            if (app == null || app.Status != (int)ApplicationStatus.Pending)
            {
                return false;
            }

            _context.Applications.Remove(app);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}