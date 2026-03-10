using Microsoft.AspNetCore.Mvc;
using RecruitmentInterviewManagementSystem.Applications.DTOs;
using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.DTO;
using RecruitmentInterviewManagementSystem.Applications.Interface;
using RecruitmentInterviewManagementSystem.Applications.Features.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace RecruitmentInterviewManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IGetCandidateID _getIdCadidate;

        public ApplicationController(
        IApplicationService applicationService,
        IGetCandidateID getCandidateID)
        {
            _applicationService = applicationService;
            _getIdCadidate = getCandidateID;
        }

        // APPLY JOB
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyJobRequestDto request)
        {
            var result = await _applicationService.ApplyForJobAsync(request);

            return Ok(new
            {
                IsSuccess = result.IsSuccess,
                Message = result.Message
            });
        }

        // GET CANDIDATE ID
        [HttpGet("candidate/{id}")]
        public async Task<IActionResult> GetIDCandidate([FromRoute] Guid id)
        {
            var idCandidate = await _getIdCadidate.GetCandidateId(id);

            return Ok(idCandidate);
        }

        // GET CVID FROM APPLICATION
        [HttpGet("{applicationId}/cv")]
        public async Task<IActionResult> GetCvIdByApplication(Guid applicationId)
        {
            var application = await _applicationService.GetApplicationByIdAsync(applicationId);

            if (application == null)
            {
                return NotFound(new { message = "Application not found" });
            }

            if (application.Cvid == Guid.Empty)
            {
                return BadRequest(new { message = "Candidate has not uploaded CV" });
            }

            return Ok(new
            {
                cvId = application.Cvid
            });
        }
    }
}