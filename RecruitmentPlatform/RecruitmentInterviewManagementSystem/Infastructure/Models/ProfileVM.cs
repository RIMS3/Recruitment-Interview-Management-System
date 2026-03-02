using System;

namespace RecruitmentInterviewManagementSystem.Models.DTO
{
    // DTO cho ứng viên
    public class CandidateUpdateDto
    {
        public DateOnly? DateOfBirth { get; set; }
        public int? Gender { get; set; }
        public string? Address { get; set; }
        public int? ExperienceYears { get; set; }
        public string? Summary { get; set; }
    }

    // DTO cho nhà tuyển dụng/công ty
    public class CompanyUpdateDto
    {
        public string? Name { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
    }
}