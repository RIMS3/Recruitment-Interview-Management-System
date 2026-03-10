using System;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Application.DTO
{
    public class ApplicationResponseDto
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;

        public Guid? ApplicationId { get; set; }

        public Guid? CvId { get; set; }
    }
}