using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class ChangeStatusJobPost : IChangeStatusJobPost
    {
        private readonly FakeTopcvContext _db;

        public ChangeStatusJobPost(FakeTopcvContext _context)
        {
            _db = _context;
        }

        public async Task<Boolean> ChangeStatusAsync(List<Guid> jobPostIds)
        {
            var jobPosts = _db.JobPosts.Where(j => jobPostIds.Contains(j.Id) && j.IsActive == true).ToList();
            if (jobPosts.Any())
            {
                foreach (var jobPost in jobPosts)
                {
                    jobPost.IsActive = false; // or any other status you want to set
                }
            }
            return  await _db.SaveChangesAsync() > 0;
        }
    }
}
