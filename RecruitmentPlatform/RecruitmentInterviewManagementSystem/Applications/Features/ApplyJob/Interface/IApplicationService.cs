namespace RecruitmentInterviewManagementSystem.Applications.Features.Application.Interface
{
    public interface IApplicationService
    {
        Task ApplyJobAsync(Guid candidateId, Guid jobId, Guid cvid);
    }
}