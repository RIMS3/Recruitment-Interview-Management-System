using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.Models
{
    public class BookingLink
    {
        public Guid Id { get; set; }

        public string Token { get; set; } = Guid.NewGuid().ToString();

        public Guid CandidateProfileId { get; set; }
        public virtual CandidateProfile CandidateProfile { get; set; } = null!;

        public Guid JobId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiredAt { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}
