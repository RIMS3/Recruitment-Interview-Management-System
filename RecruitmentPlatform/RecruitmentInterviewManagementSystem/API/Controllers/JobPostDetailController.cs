using Microsoft.AspNetCore.Mvc;
using RecruitmentInterviewManagementSystem.Applications.Features.JobPostDetail.Interface;

namespace RecruitmentInterviewManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobPostDetailController : ControllerBase
    {
        private readonly IJobPostDetailService _service;

        public JobPostDetailController(IJobPostDetailService service)
        {
            _service = service;
        }

        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetJobDetail([FromRoute]Guid jobId)
        {
            var job = await _service.GetJobPostDetailAsync(jobId);

            if (job == null)
                return NotFound("Job not found");

            return Ok(job);
        }
    }
}