using Google.Apis.Auth;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Interface
{
    public interface IGoogleTokenValidator
    {
        Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken);
    }
}