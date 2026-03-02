using RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Interface
{
    public interface IRegister
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    }
}