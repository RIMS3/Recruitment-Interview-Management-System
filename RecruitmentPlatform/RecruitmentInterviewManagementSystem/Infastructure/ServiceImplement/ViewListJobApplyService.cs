using RecruitmentInterviewManagementSystem.Models.DTOs;
using RecruitmentInterviewManagementSystem.Repositories;

public class ViewListJobApplyService : IViewListJobApplyService
{
    private readonly IViewListJobApplyRepository _repository;
    public ViewListJobApplyService(IViewListJobApplyRepository repository) => _repository = repository;

    public async Task<IEnumerable<ViewListJobApplyDTO>> GetListAppliedJobs(Guid candidateId)
        => await _repository.GetAppliedJobsByCandidateId(candidateId);

    public async Task<bool> UnapplyJob(Guid applicationId)
        => await _repository.DeleteApplication(applicationId);
}