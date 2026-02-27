using RecruitmentInterviewManagementSystem.Applications.Features.JobPost.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.JobPost.Interface;
using RecruitmentInterviewManagementSystem.Domain.Enums;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;

namespace RecruitmentInterviewManagementSystem.Applications.Features.JobPost.Services
{
    public class ViewListJobPostService : IViewListJobPost
    {
        private readonly IJobPostRepository _repository;

        public ViewListJobPostService(IJobPostRepository repository)
        {
            _repository = repository;
        }

        // Tên hàm phải khớp chính xác với Interface (ExecuteAsync)
        public async Task<IEnumerable<JobPostItemDTO>> ExecuteAsync(RequestGetViewListJobPostDTO request)
        {
            // 1. Gọi Repository lấy data từ DB
            var jobs = await _repository.GetAllAsync();

            // 2. Logic lọc (Ví dụ lọc theo tiêu đề nếu có request)
            //if (!string.IsNullOrEmpty(request.SearchTerm))
            //{
            //    jobs = jobs.Where(j => j.Title != null && j.Title.Contains(request.SearchTerm));
            //}

            // 3. Mapping sang DTO


           
            return jobs.Select(j => new JobPostItemDTO
            {
                IdJobPost = j.Id, // j.Id lấy từ Model JobPost bạn vừa gửi
                Title = j.Title,
                Location = j.Location,
                SalaryMin = j.SalaryMin,
                SalaryMax = j.SalaryMax,
                ExpireAt = j.ExpireAt,
                 Experience = j.Experience,
                JobType = (JobType?)j.JobType

            }).ToList();
        }

      
    }
}
