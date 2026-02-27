using Google.Apis.Auth;
using RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO;

namespace RecruitmentInterviewManagementSystem.Applications.Interface;

public interface IGoogleAuthService
{
    Task<LoginResponse> LoginAsync(string idToken);
}