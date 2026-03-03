using Microsoft.AspNetCore.Http;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Interface
{
    public interface IMinIOCV
    {
        Task<string> UploadAsync(IFormFile file);
        Task<string> GetUrlImage(string bucket, string imageName);
        Task DeleteAsync(string objectName);
    }
}