using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Cvs.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Cvs.Interface;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.API.Controllers;

[Route("api/cvs/{cvId:guid}/editor")]
[ApiController]
public class CvsEditorController : ControllerBase
{
    private readonly ICvService _cvService;

    public CvsEditorController(ICvService cvService)
    {
        _cvService = cvService;
    }

    [HttpGet]
    public async Task<ActionResult<CvEditorDataDto>> GetEditorData(Guid cvId)
    {
        var data = await _cvService.GetEditorDataAsync(cvId);
        if (data == null) return NotFound("Không tìm thấy CV.");
        return Ok(data);
    }

    [HttpPut]
    public async Task<ActionResult<CvEditorDataDto>> UpdateEditorData(Guid cvId, [FromBody] UpdateCvEditorRequest request)
    {
        try
        {
            var updatedData = await _cvService.UpdateEditorDataAsync(cvId, request);
            if (updatedData == null) return NotFound("Không tìm thấy CV.");
            return Ok(updatedData);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}