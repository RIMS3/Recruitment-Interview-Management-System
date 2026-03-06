using Microsoft.AspNetCore.Mvc;
using RecruitmentInterviewManagementSystem.Applications.DTOs;
using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.Interface;
using RecruitmentInterviewManagementSystem.Applications.Interface;
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

        public ApplicationController(IApplicationService applicationService , IGetCandidateID getCandidateID)
        {
            _applicationService = applicationService;
            _getIdCadidate = getCandidateID;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyJobRequestDto request)
        {
            
                var result = await _applicationService.ApplyForJobAsync(request);

            return Ok(new {IsSuccess  = result.IsSuccess, Message  = result.Message});
              
                   }

        [HttpGet("candidate/{id}")] 
        public async Task<IActionResult> GetIDCandidate([FromRoute]Guid id)
        {

            var idCandidate = await _getIdCadidate.GetCandidateId(id);

            return Ok(idCandidate);
        }


        }
}