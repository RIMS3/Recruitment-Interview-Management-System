using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.DTO;

namespace RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.Interface
{
    public interface IApplicationService
    {
        Task<string> ApplyForJobAsync(ApplyJobRequestDto request);
    }
}