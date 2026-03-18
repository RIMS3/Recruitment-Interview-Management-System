namespace RecruitmentInterviewManagementSystem.Applications.ChangePassword
{
    public interface IChangePasswrod
    {
        Task<ChangePasswordResponse> Execute(ChangePasswordRequest request);
    }

    public class ChangePasswordRequest
    {
        public string Email { get; set; }= null!;
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class ChangePasswordResponse
    {
        public bool Success { get; set; } 
        public string Message { get; set; } = null!;
    }
}
