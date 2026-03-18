using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.ChangePassword;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class ChangePasswrod : IChangePasswrod
    {
        private readonly FakeTopcvContext _db;

        public ChangePasswrod(FakeTopcvContext fakeTopcvContext)
        {
            _db  = fakeTopcvContext;
        }

        public async Task<ChangePasswordResponse> Execute(ChangePasswordRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = "Tài khoản không tồn tại."
                };
            }

            var isValid = PasswordHasher.VerifyPassword(
                request.OldPassword,
                user.PasswordHash,
                user.Salt
            );

            if (!isValid)
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = "Mật khẩu cũ không chính xác."
                };
            }

            
            if (request.OldPassword == request.NewPassword)
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = "Mật khẩu mới không được trùng mật khẩu cũ."
                };
            }

            var validateMessage = ValidatePassword(request.NewPassword);
            if (validateMessage != null)
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = validateMessage
                };
            }

            var (hash, salt) = PasswordHasher.HashPassword(request.NewPassword);

            user.PasswordHash = hash;
            user.Salt = salt;

            var result = await _db.SaveChangesAsync();

            if (result > 0)
            {
                return new ChangePasswordResponse
                {
                    Success = true,
                    Message = "Đổi mật khẩu thành công."
                };
            }

            return new ChangePasswordResponse
            {
                Success = false,
                Message = "Có lỗi xảy ra, vui lòng thử lại."
            };
        }

        private string? ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Mật khẩu không được để trống.";

            if (password.Length < 6)
                return "Mật khẩu phải có ít nhất 6 ký tự.";

            if (!password.Any(char.IsUpper))
                return "Mật khẩu phải có ít nhất 1 chữ in hoa.";

            if (!password.Any(char.IsLower))
                return "Mật khẩu phải có ít nhất 1 chữ thường.";

            if (!password.Any(char.IsDigit))
                return "Mật khẩu phải có ít nhất 1 chữ số.";

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                return "Mật khẩu phải có ít nhất 1 ký tự đặc biệt.";

            return null; 
        }
    }
}
