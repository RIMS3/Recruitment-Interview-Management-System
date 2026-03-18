using RecruitmentInterviewManagementSystem.Applications.Notifications.DTO;

namespace RecruitmentInterviewManagementSystem.Applications.Notifications.Producers
{
    public interface IInformScheduleInterivewProducer
    {
        Task Execute(NotificationDTOS request);
    }
}
