namespace RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
