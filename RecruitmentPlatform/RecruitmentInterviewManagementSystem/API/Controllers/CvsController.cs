using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio.DataModel.Args;
using RecruitmentInterviewManagementSystem.Applications.Features.Cvs.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Cvs.Interface;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CvsController : ControllerBase
{
    private readonly ICvService _cvService;
    private readonly IMinIOCV _minIO;

    public CvsController(ICvService cvService, IMinIOCV minIOCV)
    {
        _cvService = cvService;
        _minIO = minIOCV;
    }

    [HttpGet("candidate/{candidateId:guid}")]
    public async Task<ActionResult<IEnumerable<CvSummaryDto>>> GetByCandidate(Guid candidateId)
    {
        var cvs = await _cvService.GetCvsByCandidateAsync(candidateId);
        return Ok(cvs);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CvDetailDto>> GetById(Guid id)
    {
        var cv = await _cvService.GetCvByIdAsync(id);
        if (cv == null) return NotFound("Không tìm thấy CV.");
        return Ok(cv);
    }

    [HttpPost]
    public async Task<ActionResult<CvDetailDto>> Create([FromForm] CreateCvRequest request)
    {
        try
        {
            var created = await _cvService.CreateCvAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CvDetailDto>> Update(Guid id, [FromForm] UpdateCvRequest request)
    {
        try
        {
            var updated = await _cvService.UpdateCvAsync(id, request);
            if (updated == null) return NotFound("Không tìm thấy CV.");
            return Ok(updated);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _cvService.DeleteCvAsync(id);
        if (!isDeleted) return NotFound("Không tìm thấy CV.");
        return Ok(new { message = "Xóa CV thành công." });
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> DownloadCv(Guid id)
    {
        try
        {
            var fileData = await _cvService.DownloadCvAsync(id);
            if (fileData == null) return NotFound("Không tìm thấy file CV.");

            return File(fileData.Value.Stream, fileData.Value.ContentType, fileData.Value.FileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi khi tải file: {ex.Message}");
        }
    }


    [HttpGet("images")]
    public async Task<IActionResult> GetImageCV()
    {
        var template1 = await _minIO.GetUrlImage("avatars", "template1.png");
        var template2 = await _minIO.GetUrlImage("avatars", "template2.png");
        var template3 = await _minIO.GetUrlImage("avatars", "template3.png");

        var images = new List<string>
    {
        template1,
        template2,
        template3
    };

        return Ok(images);
    }
}