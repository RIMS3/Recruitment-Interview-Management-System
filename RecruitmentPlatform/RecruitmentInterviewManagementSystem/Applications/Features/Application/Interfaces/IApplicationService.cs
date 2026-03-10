using RecruitmentInterviewManagementSystem.Applications.Features.Application.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.DTO;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Application.Interfaces
{
    public interface IApplicationService
    {
        // APPLY JOB
        Task<ApplicationResponseDto> ApplyForJobAsync(ApplyJobRequestDto request);

        // GET APPLICATION
        Task<Models.Application?> GetApplicationByIdAsync(Guid applicationId);
    }
}