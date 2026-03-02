using System;

namespace RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.DTO
{
    public class ApplyJobRequestDto
    {
        public Guid JobId { get; set; }
        public Guid CandidateId { get; set; }
        public Guid CvId { get; set; }
    }
}