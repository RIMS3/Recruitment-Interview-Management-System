using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Application.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Application.Interfaces;
using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.DTO;
using RecruitmentInterviewManagementSystem.Models;
using ApplicationEntity = RecruitmentInterviewManagementSystem.Models.Application;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Application.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly FakeTopcvContext _context;

        public ApplicationService(FakeTopcvContext context)
        {
            _context = context;
        }

        public async Task<ApplicationEntity?> GetApplicationByIdAsync(Guid applicationId)
        {
            return await _context.Applications
                .FirstOrDefaultAsync(a => a.Id == applicationId);
        }

        public async Task<ApplicationResponseDto> ApplyForJobAsync(ApplyJobRequestDto request)
        {
            try
            {
                var application = new ApplicationEntity
                {
                    Id = Guid.NewGuid(),
                    JobId = request.JobId,
                    CandidateId = request.CandidateId,
                    Cvid = request.CvId,
                    AppliedAt = DateTime.UtcNow
                };

                _context.Applications.Add(application);
                await _context.SaveChangesAsync();

                return new ApplicationResponseDto
                {
                    IsSuccess = true,
                    Message = "Apply job successfully",
                    ApplicationId = application.Id
                };
            }
            catch (Exception ex)
            {
                return new ApplicationResponseDto
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}