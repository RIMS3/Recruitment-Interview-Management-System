namespace RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.Interface
{
    public interface IApplicationService
    {
        Task ApplyJobAsync(Guid candidateId, Guid jobId, Guid cvId);
    }
}