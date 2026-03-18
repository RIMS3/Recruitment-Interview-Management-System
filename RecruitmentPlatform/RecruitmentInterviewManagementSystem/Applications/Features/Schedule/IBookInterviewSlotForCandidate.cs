namespace RecruitmentInterviewManagementSystem.Applications.Features.Schedule
{
    public interface IBookInterviewSlotForCandidate
    {
        Task<(bool IsSuccess, string Message)> ExecuteAsync(string token, Guid slotId);
    }
}

