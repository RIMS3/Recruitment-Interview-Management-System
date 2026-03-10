using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.DTO;
using static RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement.ApplyJobService;

namespace RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.Interface
{
    public interface IApplyJobService
    {
        Task<ResultStatus> ApplyForJobAsync(ApplyJobRequestDto request);
    }
}