using RecruitmentInterviewManagementSystem.Domain.Entities;

public interface IUserRepository
{
    Task<UserEntity?> GetByEmailAsync(string email);

    Task<UserEntity> CreateGoogleUserAsync(string email, string fullName);
}