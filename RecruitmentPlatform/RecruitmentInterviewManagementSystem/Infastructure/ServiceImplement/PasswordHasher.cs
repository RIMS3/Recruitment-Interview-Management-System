using System.Security.Cryptography;
using System.Text;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public static class PasswordHasher
    {
        public static (string hash, string salt) HashPassword(string password)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);

            using var sha256 = SHA256.Create();
            var combined = Encoding.UTF8.GetBytes(password + salt);
            var hashBytes = sha256.ComputeHash(combined);

            return (Convert.ToBase64String(hashBytes), salt);
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            using var sha256 = SHA256.Create();
            var combined = Encoding.UTF8.GetBytes(password + storedSalt);
            var hashBytes = sha256.ComputeHash(combined);

            return Convert.ToBase64String(hashBytes) == storedHash;
        }
    }
}