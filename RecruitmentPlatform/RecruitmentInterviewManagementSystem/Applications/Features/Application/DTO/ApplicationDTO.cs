using RecruitmentInterviewManagementSystem.Domain.Enums;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Application.DTO
{
    public class ApplicationDTO
    {
        public Guid ApplicationId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string? CandidateAvatar { get; set; } // avt
        public string JobTitle { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public int Status { get; set; }
        public string CvUrl { get; set; } = string.Empty;
    }

    public class UpdateStatusDTO
    {
        public ApplicationStatus NewStatus { get; set; }
    }
}