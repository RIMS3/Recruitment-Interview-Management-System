using Microsoft.AspNetCore.Mvc;
using RecruitmentInterviewManagementSystem.Applications.Features.SavedJobs;
using RecruitmentInterviewManagementSystem.Applications.Features.SavedJobs.DTO;

namespace RecruitmentInterviewManagementSystem.API.Controllers;

[Route("api/saved-jobs")]
[ApiController]
public class SavedJobsController : ControllerBase
{
    private readonly ISavedJobService _savedJobService;

    // Inject Service thay vì DbContext
    public SavedJobsController(ISavedJobService savedJobService)
    {
        _savedJobService = savedJobService;
    }

    [HttpGet("{candidateId:guid}")]
    public async Task<ActionResult<IEnumerable<SavedJobItemDto>>> GetSavedJobs(Guid candidateId)
    {
        var items = await _savedJobService.GetSavedJobsAsync(candidateId);
        return Ok(items);
    }

    [HttpGet("{candidateId:guid}/ids")]
    public async Task<ActionResult<IEnumerable<Guid>>> GetSavedJobIds(Guid candidateId)
    {
        var jobIds = await _savedJobService.GetSavedJobIdsAsync(candidateId);
        return Ok(jobIds);
    }

    [HttpPost]
    public async Task<IActionResult> SaveJob([FromBody] SaveJobRequest request)
    {
        var result = await _savedJobService.SaveJobAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(new { saved = true, message = result.Message });
    }

    [HttpDelete]
    public async Task<IActionResult> UnsaveJob([FromBody] SaveJobRequest request)
    {
        var result = await _savedJobService.UnsaveJobAsync(request);

        if (!result.IsSuccess)
            return NotFound(new { saved = false, message = result.Message });

        return Ok(new { saved = false, message = result.Message });
    }

    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleSavedJob([FromBody] SaveJobRequest request)
    {
        var result = await _savedJobService.ToggleSavedJobAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(new { saved = result.IsSaved, message = result.Message });
    }
}