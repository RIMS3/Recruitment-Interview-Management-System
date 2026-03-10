namespace RecruitmentInterviewManagementSystem.Applications.Features.Cvs.DTO
{
    public class CvAvatarResponseDto
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public string? MimeType { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
