using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.DTO;
using static RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement.ApplicationService;

namespace RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.Interface
{
    public interface IApplicationService
    {
        Task<ResultStatus> ApplyForJobAsync(ApplyJobRequestDto request);
    }
}