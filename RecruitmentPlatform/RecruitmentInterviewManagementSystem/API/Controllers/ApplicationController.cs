//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;
//using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.DTO;
//using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.Interface;

//namespace RecruitmentInterviewManagementSystem.API.Controllers
//{
//    [ApiController]
//    [Route("api/applications")]
//    [Authorize(Roles = "Candidate")]
//    public class ApplicationController : ControllerBase
//    {
//        private readonly IApplicationService _service;

//        public ApplicationController(IApplicationService service)
//        {
//            _service = service;
//        }

//        [HttpPost("apply")]
//        public async Task<IActionResult> Apply([FromBody] ApplyJobDTO request)
//        {
//            var candidateIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

//            if (candidateIdClaim == null)
//                return Unauthorized();

//            var candidateId = Guid.Parse(candidateIdClaim);

//            await _service.ApplyJobAsync(candidateId, request.JobId, request.Cvid);

//            return Ok(new { message = "Apply successfully" });
//        }
//    }
//}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.ApplyJob.Interface;

namespace RecruitmentInterviewManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/applications")]
    //[Authorize(Roles = "Candidate")]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _service;

        public ApplicationController(IApplicationService service)
        {
            _service = service;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyJobDTO request)
        {
            var candidateIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(candidateIdClaim, out var candidateId))
                return Unauthorized();

            if (request.JobId == Guid.Empty || request.Cvid == Guid.Empty)
                return BadRequest("Invalid request data.");

            try
            {
                await _service.ApplyJobAsync(candidateId, request.JobId, request.Cvid);
                return Ok(new { message = "Apply successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message }); // 409
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}