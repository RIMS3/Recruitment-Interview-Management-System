namespace RecruitmentInterviewManagementSystem.Applications.Features.Advertisement.DTO
{
    public class AdvertisementDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public int Duration { get; set; }
        public string? LinkUrl { get; set; }
        public bool IsPopup { get; set; }
    }

    public class CreateAdvertisementDTO
    {
        public string Title { get; set; }
        public IFormFile ImageFile { get; set; }
        public int Duration { get; set; } = 5;
        public string? LinkUrl { get; set; }
        public bool IsPopup { get; set; }
    }

    public class UpdateAdvertisementDTO
    {
        public string Title { get; set; }
        public IFormFile? ImageFile { get; set; }
        public int Duration { get; set; }
        public string? LinkUrl { get; set; }
        public bool IsPopup { get; set; }
    }
}
