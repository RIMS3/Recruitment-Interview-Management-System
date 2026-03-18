namespace RecruitmentInterviewManagementSystem.Applications.Notifications.DTO
{
    public class NotificationDTOS
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string? Link { get; set; }

        public string? Titel { get; set; }

        public string? Message { get; set; }

        public string TypeService { get; set; } = "Email";
    }
}
