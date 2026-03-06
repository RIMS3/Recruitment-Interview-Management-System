using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.DTOs;
using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.Interface;
using RecruitmentInterviewManagementSystem.Applications.Interface;
using RecruitmentInterviewManagementSystem.Domain.Enums;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;
using System;
using System.Threading.Tasks;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly FakeTopcvContext _db;

        public ApplicationService(IApplicationRepository applicationRepository,FakeTopcvContext fakeTopcvContext)
        {
            _applicationRepository = applicationRepository;
            _db = fakeTopcvContext;
        }
        public async Task<ResultStatus> ApplyForJobAsync(ApplyJobRequestDto request)
        {
            var alreadyApplied = await _db.Applications.AnyAsync(a =>
                a.CandidateId == request.CandidateId &&
                a.JobId == request.JobId);

            if (alreadyApplied)
            {
                return new ResultStatus
                {

                    IsSuccess = false,
                    Message = "Bạn đã ứng tuyển vào công việc này rồi!"
                };
            }

            var newApplication = new Application
            {
                Id = Guid.NewGuid(),
                JobId = request.JobId,
                CandidateId = request.CandidateId,
                Cvid = request.CvId,
                Status = (int)ApplicationStatus.Pending,
                AppliedAt = DateTime.UtcNow
            };

            await _db.Applications.AddAsync(newApplication);
            await _db.SaveChangesAsync();

            return new ResultStatus
            {
                IsSuccess = true,
                Message = "Ứng tuyển thành công!"
            };
        }


        public class ResultStatus
        {
            public bool IsSuccess { get; set; }

            public string Message { get; set; }
        }
    }
}