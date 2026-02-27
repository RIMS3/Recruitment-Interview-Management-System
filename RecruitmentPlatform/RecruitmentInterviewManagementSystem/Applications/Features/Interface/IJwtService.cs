using RecruitmentInterviewManagementSystem.Domain.Entities;

public interface IJwtService
{
    string GenerateToken(UserEntity user);
}