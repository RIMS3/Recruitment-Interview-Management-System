namespace RecruitmentInterviewManagementSystem.Applications.Features.Notifications
{
    public interface INotifications
    {
        public string TypeService { get; }
        Task<bool> SendRegisterAccount(MessageBody request);
    }

    public class MessageBody
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
