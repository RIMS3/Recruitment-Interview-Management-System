using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class ViewListJobApplyController : ControllerBase
{
    private readonly IViewListJobApplyService _service;
    public ViewListJobApplyController(IViewListJobApplyService service) => _service = service;

    [HttpGet("candidate/{candidateId}")]
    public async Task<IActionResult> GetList(Guid candidateId) => Ok(await _service.GetListAppliedJobs(candidateId));

    [HttpDelete("unapply/{applicationId}")]
    public async Task<IActionResult> Unapply(Guid applicationId)
    {
        var success = await _service.UnapplyJob(applicationId);
        return success ? Ok(new { isSuccess = true, message = "Đã hủy ứng tuyển!" })
                       : BadRequest(new { isSuccess = false, message = "Lỗi khi hủy hồ sơ!" });
    }
}