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

        public ApplicationController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyJobRequestDto request)
        {
            try
            {
                var result = await _applicationService.ApplyForJobAsync(request);
                return Ok(new { success = true, message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}