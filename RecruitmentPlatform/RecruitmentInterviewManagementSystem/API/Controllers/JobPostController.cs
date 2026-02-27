using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.JobPost.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.JobPost.Interface;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;


namespace RecruitmentInterviewManagementSystem.API.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobPostController : ControllerBase
    {
        private readonly IViewListJobPost _viewService;
        private readonly IJobPostRepository _jobPostRepository; // THÊM DÒNG NÀY

        // Sửa Constructor để nhận thêm Repository
        public JobPostController(IViewListJobPost viewService, IJobPostRepository jobPostRepository)
        {
            _viewService = viewService;
            _jobPostRepository = jobPostRepository; // Gán giá trị vào biến private
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs([FromQuery] RequestGetViewListJobPostDTO request)
        {
            var result = await _viewService.ExecuteAsync(request);
            return Ok(result);
        }
        
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<JobPost>>> GetFilteredJobs([FromQuery] JobPostFilterRequest filter)
        {
            try
            {
                var jobs = await _jobPostRepository.GetFilteredJobsAsync(filter);
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations()
        {
            var locations = await _jobPostRepository.GetLocationsAsync();
            return Ok(locations);
        }
    }
}
