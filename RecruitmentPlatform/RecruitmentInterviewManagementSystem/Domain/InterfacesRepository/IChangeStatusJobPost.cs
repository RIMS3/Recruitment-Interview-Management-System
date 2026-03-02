namespace RecruitmentInterviewManagementSystem.Domain.InterfacesRepository
{
    public interface IChangeStatusJobPost
    {
          Task<Boolean> ChangeStatusAsync(List<Guid> jobPostId);
    }
}
