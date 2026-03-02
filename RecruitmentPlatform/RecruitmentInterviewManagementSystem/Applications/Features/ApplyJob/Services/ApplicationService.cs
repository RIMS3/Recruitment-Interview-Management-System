using RecruitmentInterviewManagementSystem.Applications.Features.Application.Interface;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Application.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _repository;

        public ApplicationService(IApplicationRepository repository)
        {
            _repository = repository;
        }

        public async Task ApplyJobAsync(Guid candidateId, Guid jobId, Guid cvid)
        {
            var isApplied = await _repository
                .IsAlreadyAppliedAsync(jobId, candidateId);

            if (isApplied)
                throw new Exception("You already applied this job.");

            var application = new ApplicationEntity
            {
                JobId = jobId,
                CandidateId = candidateId,
                Cvid = cvid,
                Status = "Pending"
                // AppliedAt t? set t? DB (getdate())
            };

            await _repository.AddAsync(application);
        }
    }
}