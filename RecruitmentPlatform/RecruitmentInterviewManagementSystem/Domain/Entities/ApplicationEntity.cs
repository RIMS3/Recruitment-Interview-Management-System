using System;

namespace RecruitmentInterviewManagementSystem.Models
{
    public partial class ApplicationEntity
    {
        public Guid Id { get; set; }

        public Guid JobId { get; set; }
        public Guid CandidateId { get; set; }
        public Guid Cvid { get; set; }

        public string Status { get; set; }

        public DateTime AppliedAt { get; set; }

        public virtual CandidateProfile Candidate { get; set; }
        public virtual Cv Cv { get; set; }
        public virtual JobPost Job { get; set; }
    }
}