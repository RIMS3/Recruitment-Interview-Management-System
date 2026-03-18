using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.ChangePassword;
using RecruitmentInterviewManagementSystem.Applications.Notifications.Producers;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class ResetPassword : IResetPassword
    {
        private readonly FakeTopcvContext _db;
        private readonly IInformScheduleInterivewProducer _informEmail;

        public ResetPassword(FakeTopcvContext fakeTopcvContext, IInformScheduleInterivewProducer informScheduleInterivewProducer)
        {
            _db = fakeTopcvContext;
            _informEmail = informScheduleInterivewProducer;
        }

        public async Task<bool> Execute(ResetPasswordRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (user == null)
                return false;


            var newPassword = Guid.NewGuid().ToString("N").Substring(0, 8);


            var (hash, salt) = PasswordHasher.HashPassword(newPassword);


            user.PasswordHash = hash;
            user.Salt = salt;

            var result = await _db.SaveChangesAsync();

            if (result > 0)
            {
                await _informEmail.Execute(new Applications.Notifications.DTO.NotificationDTOS
                {
                    Name = user.FullName ?? "Người đẹp",
                    Email = user.Email,
                    Titel = "Reset Password",
                    Message = $"Xin chào {user.FullName ?? "bạn"},\n\n" +
          $"Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.\n\n" +
          $" Mật khẩu mới của bạn là: {newPassword}\n\n" +
          $"Vì lý do bảo mật, vui lòng đăng nhập và thay đổi mật khẩu ngay sau khi sử dụng.\n\n" +
          $"Nếu bạn không thực hiện yêu cầu này, hãy liên hệ với chúng tôi ngay lập tức.\n\n" +
          $"Trân trọng,\nĐội ngũ hỗ trợ."
                });
            }

            return result > 0;
        }
    }
}

