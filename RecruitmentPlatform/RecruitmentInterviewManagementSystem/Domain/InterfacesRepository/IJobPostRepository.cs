using RecruitmentInterviewManagementSystem.Applications.Features.JobPost.DTO;
using RecruitmentInterviewManagementSystem.Domain.Entities;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Domain.InterfacesRepository
{
    public interface IJobPostRepository
    {
        Task<IEnumerable<JobPost>> GetAllAsync();

        // SỬA TẠI ĐÂY: Trả về JobPostItemDTO thay vì JobPost
        Task<IEnumerable<JobPostItemDTO>> GetFilteredJobsAsync(JobPostFilterRequest filter);

        Task<List<string>> GetLocationsAsync();
    }
}