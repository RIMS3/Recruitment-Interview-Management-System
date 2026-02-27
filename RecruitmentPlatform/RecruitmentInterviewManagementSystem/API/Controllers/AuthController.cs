using Microsoft.AspNetCore.Mvc;
using RecruitmentInterviewManagementSystem.Applications.Interface;
using RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO;
using Microsoft.AspNetCore.Authorization;

namespace RecruitmentInterviewManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;

    public AuthController(IGoogleAuthService googleAuthService)
    {
        _googleAuthService = googleAuthService;
    }
    [AllowAnonymous]
    [HttpPost("google")]

    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        if (string.IsNullOrEmpty(request.IdToken))
        {
            return BadRequest("IdToken is required");
        }

        var result = await _googleAuthService.LoginAsync(request.IdToken);

        return Ok(new
        {
            accessToken = result.AccessToken,
            email = result.Email,
            fullName = result.FullName
        });
    }
}