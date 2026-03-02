namespace RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO
{
    public class RegisterResponse
    {
        public string AccessToken { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }
}