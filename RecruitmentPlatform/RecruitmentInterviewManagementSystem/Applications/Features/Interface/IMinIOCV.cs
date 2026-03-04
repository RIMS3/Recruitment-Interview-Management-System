using Microsoft.AspNetCore.Http;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Interface
{
    public interface IMinIOCV
    {
        Task<string> UploadAsync(IFormFile file, string bucketName);
        Task<string?> GetUrlImage(string bucketName, string objectName);
        Task DeleteAsync(string objectName, string bucketName);
        Task GetObjectStreamAsync(string bucketName, string objectName, Action<Stream> callback);
    }
}