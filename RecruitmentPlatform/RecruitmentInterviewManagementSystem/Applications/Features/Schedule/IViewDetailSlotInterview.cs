using RecruitmentInterviewManagementSystem.API.DTOs;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Schedule
{
    public interface IViewDetailSlotInterview
    {
        Task<InterviewSlotDetailDto?> GetSlotDetailAsync(Guid slotId);
    }
}
