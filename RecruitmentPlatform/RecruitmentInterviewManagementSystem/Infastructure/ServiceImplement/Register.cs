using RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Domain.Entities;
using RecruitmentInterviewManagementSystem.Domain.Enums;
using RecruitmentInterviewManagementSystem.Infastructure.Repository;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class Register : IRegister
    {
        private readonly FakeTopcvContext _context;
        private readonly IJwtService _jwtService;

        public Register(FakeTopcvContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = _context.Users
                .FirstOrDefault(x => x.Email == request.Email);

            if (existingUser != null)
                throw new Exception("Email already exists");

            var (hash, salt) = PasswordHasher.HashPassword(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = hash,
                Salt = salt,
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userEntity = new UserEntity(
                user.Id,
                user.Email,
                user.FullName!,
                (Role)(user.Role ?? 0),
                user.IsActive ?? true
            );

            var token = _jwtService.GenerateToken(userEntity);

            return new RegisterResponse
            {
                AccessToken = token,
                Email = user.Email,
                FullName = user.FullName!
            };
        }
    }
}