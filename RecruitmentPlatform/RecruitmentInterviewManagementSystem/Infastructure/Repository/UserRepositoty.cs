using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Domain.Entities;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly FakeTopcvContext _context;

        public UserRepository(FakeTopcvContext context)
        {
            _context = context;
        }

        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return null;

            return MapToDomain(user);
        }

        public async Task<UserEntity> CreateGoogleUserAsync(string email, string fullName)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = fullName,
                IsActive = true,
                Role = 0,
                PasswordHash = Guid.NewGuid().ToString() // 👈 fake password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return MapToDomain(user);
        }

        private static UserEntity MapToDomain(User user)
        {
            return new UserEntity(
                user.Id,
                user.Email,
                user.FullName,
                user.Role ?? 0,
                user.IsActive ?? false
            );
        }
    }
}