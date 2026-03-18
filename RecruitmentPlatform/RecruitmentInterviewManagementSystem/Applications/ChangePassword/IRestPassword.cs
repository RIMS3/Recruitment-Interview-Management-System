namespace RecruitmentInterviewManagementSystem.Applications.ChangePassword
{
    public interface IResetPassword
    {
        Task<bool> Execute(ResetPasswordRequest email);
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = null!;
    }
}
