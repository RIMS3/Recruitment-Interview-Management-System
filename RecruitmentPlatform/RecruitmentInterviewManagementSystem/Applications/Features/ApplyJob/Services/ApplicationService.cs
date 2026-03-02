using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.Interface;
using RecruitmentInterviewManagementSystem.Domain.Enums;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _repository;

        public ApplicationService(IApplicationRepository repository)
        {
            _repository = repository;
        }

        public async Task ApplyJobAsync(Guid candidateId, Guid jobId, Guid cvId)
        {
            var isApplied = await _repository
                .IsAlreadyAppliedAsync(jobId, candidateId);

            if (isApplied)
                throw new Exception("You already applied this job.");

            var application = new Application
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                CandidateId = candidateId,
                Cvid = cvId,
                Status = (int)ApplicationStatus.Pending,
                AppliedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(application);
        }
    }
}