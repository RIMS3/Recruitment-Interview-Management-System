namespace RecruitmentInterviewManagementSystem.Infastructure.Models
{
    public class Advertisement
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageName { get; set; }
        public int Duration { get; set; } = 5;
        public string? LinkUrl { get; set; }
        public bool IsPopup { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
