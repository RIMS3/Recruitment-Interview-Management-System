namespace RecruitmentInterviewManagementSystem.Applications.Features.Interface
{
    public interface IMinIOCV
    {
        Task<string> UploadAsync(IFormFile file);
        Task DeleteAsync(string objectName);

        Task<string> GetUrlImage(string bucket, string imageName);
    }
}
