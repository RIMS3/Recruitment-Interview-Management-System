using System;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO
{
    public class EmployerProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? CompanyName { get; set; }
        public string? AvatarUrl { get; set; }
        public Guid CompanyId { get; set; }
        public string? Position { get; set; }
        public string? TaxCode { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public string? CompanySize { get; set; }
        public string? Description { get; set; }

        // Đường dẫn ảnh hiển thị trên React
        public string? LogoUrl { get; set; }
    }
}