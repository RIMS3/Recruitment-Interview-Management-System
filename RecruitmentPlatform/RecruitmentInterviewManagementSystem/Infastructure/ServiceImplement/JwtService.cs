using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RecruitmentInterviewManagementSystem.Domain.Entities;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Domain.Enums;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(UserEntity user)
        {
            var secret = _configuration["Authentication:Jwt:Secret"]
                         ?? throw new Exception("JWT Secret not configured");

            var issuer = _configuration["Authentication:Jwt:Issuer"];
            var audience = _configuration["Authentication:Jwt:Audience"];

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role,((Role)user.Role).ToString()),
                new Claim("fullName", user.FullName)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}