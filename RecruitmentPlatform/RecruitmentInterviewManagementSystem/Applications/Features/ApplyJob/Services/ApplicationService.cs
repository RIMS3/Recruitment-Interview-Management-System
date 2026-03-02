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

        public ApplicationService(IApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        public async Task<string> ApplyForJobAsync(ApplyJobRequestDto request)
        {
            // 1. Kiểm tra đã apply chưa
            var isAlreadyApplied = await _applicationRepository.CheckApplicationExistsAsync(request.JobId, request.CandidateId);

            if (isAlreadyApplied)
            {
                throw new Exception("Bạn đã ứng tuyển vào vị trí này rồi.");
            }

            // 2. Tạo bản ghi Application
            var newApplication = new Application
            {
                Id = Guid.NewGuid(),
                JobId = request.JobId,
                CandidateId = request.CandidateId,
                Cvid = request.CvId,
                Status = (int)ApplicationStatus.Pending,
                AppliedAt = DateTime.Now,
                // Đảm bảo không có trường nào khác bị bắt buộc (Required) trong DB mà đang bị null
            };

            // 3. Lưu
            await _applicationRepository.CreateApplicationAsync(newApplication);
            await _applicationRepository.SaveChangesAsync();

            return "Ứng tuyển thành công!";
        }
    }
}