namespace RecruitmentInterviewManagementSystem.Domain.Entities
{
    public class UserEntity
    {
        public Guid Id { get; }
        public string Email { get; }
        public string FullName { get; }
        public int Role { get; }
        public bool IsActive { get; }

        public UserEntity(Guid id, string email, string fullName, int role, bool isActive)
        {
            Id = id;
            Email = email;
            FullName = fullName;
            Role = role;
            IsActive = isActive;
        }
    }
}