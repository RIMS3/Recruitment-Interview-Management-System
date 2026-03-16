using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.DTOs;
using RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Domain.Entities;
using RecruitmentInterviewManagementSystem.Domain.Enums;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class Login : ILogin
    {
        private readonly FakeTopcvContext _context;
        private readonly IJwtService _jwtService;

        public Login(FakeTopcvContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<LoginResponse?> LoginAsync(RequestLogin request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            var idUser = Guid.Empty;

            var idCompany = Guid.Empty; 

            if (user.Role == (int)Role.EMPLOYERER)
            {
                idUser = await _context.EmployerProfiles.Where(s => s.UserId == user.Id).Select(s => s.Id).FirstOrDefaultAsync();

                 idCompany = await _context.EmployerProfiles.Where(s => s.UserId == user.Id).Select(s => s.CompanyId).FirstOrDefaultAsync();

            }
            else if (user.Role == (int)Role.CANDIDATE)
            {

                idUser = await _context.CandidateProfiles.Where(s => s.UserId == user.Id).Select(s => s.Id).FirstOrDefaultAsync();
            }
            else if (user.Role == (int)Role.ADMIN)
            {
                idUser = user.Id;
            }

            // ❌ KHÔNG THROW EXCEPTION NỮA
            if (user == null)
                    return null;

            var isValid = PasswordHasher.VerifyPassword(
                request.Password,
                user.PasswordHash,
                user.Salt);

            if (!isValid)
                return null;

            // Optional: kiểm tra tài khoản bị khóa
            if (user.IsActive == false)
                return null;

            var userEntity = new UserEntity(
               idUser,
                user.Email,
                user.FullName!,
                (Role)(user.Role ?? 0),
                user.IsActive ?? true
            );

            var accessToken = _jwtService.GenerateToken(userEntity);
            var refreshToken = Guid.NewGuid().ToString();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                JwtId = Guid.NewGuid().ToString(),
                IsUsed = false,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(7)
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                UserId = user.Id,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Email = user.Email,
                FullName = user.FullName!,
                Role = user.Role ?? 0,
                IdCompany = idCompany
            };
        }
    }
}