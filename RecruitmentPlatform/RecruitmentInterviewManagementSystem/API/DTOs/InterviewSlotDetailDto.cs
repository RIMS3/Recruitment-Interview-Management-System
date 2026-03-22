namespace RecruitmentInterviewManagementSystem.API.DTOs
{
    public class InterviewSlotDetailDto
    {
        // Thông tin Slot
        public Guid SlotId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsBooked { get; set; }

        // Thông tin Cuộc phỏng vấn
        public Guid? InterviewId { get; set; }
        public string? Status { get; set; }
        public decimal? TechnicalScore { get; set; }
        public decimal? SoftSkillScore { get; set; }
        public string? Decision { get; set; }

        // Thông tin Ứng viên (Candidate)
        public Guid? CandidateId { get; set; }
        public string? CandidateAvatarUrl { get; set; }
        public int? ExperienceYears { get; set; }

        // Thêm thông tin User của Candidate
        public string? CandidateName { get; set; }
        public string? CandidateEmail { get; set; }
        public string? CandidatePhone { get; set; }

        // Thông tin Công việc (Job)
        public Guid? JobId { get; set; }
        public string? JobTitle { get; set; }
        public string? JobLocation { get; set; }
    }
}
