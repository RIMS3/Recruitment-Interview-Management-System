namespace RecruitmentInterviewManagementSystem.Applications.Features.Banner.DTO
{
    public class BannerDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
    }

    public class CreateBannerDTO
    {
        public string Title { get; set; }
        public IFormFile ImageFile { get; set; }
    }

    public class UpdateBannerDTO
    {
        public string Title { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}   
