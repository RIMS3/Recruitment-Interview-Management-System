using System;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Application.DTO
{
    public class ApplyJobRequest
    {
        public Guid JobId { get; set; }
        public Guid Cvid { get; set; }
    }
}