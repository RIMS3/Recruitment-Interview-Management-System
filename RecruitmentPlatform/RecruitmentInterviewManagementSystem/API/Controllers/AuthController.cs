using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.DTOs;
using RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Applications.Interface;
using RecruitmentInterviewManagementSystem.Domain.Entities;
using RecruitmentInterviewManagementSystem.Domain.Enums;
using RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement;
using RecruitmentInterviewManagementSystem.Models;


namespace RecruitmentInterviewManagementSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IRegister _register;
    private readonly ILogin _login;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly FakeTopcvContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(
    IGoogleAuthService googleAuthService,
    IRegister register,
    ILogin login,
    FakeTopcvContext context,
    IJwtService jwtService)
    {
        _googleAuthService = googleAuthService;
        _register = register;
        _login = login;
        _context = context;
        _jwtService = jwtService;
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

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _register.RegisterAsync(request);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] RequestLogin request)
    {
        var result = await _login.LoginAsync(request);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (storedToken == null
    || storedToken.IsUsed == true
    || storedToken.IsRevoked == true)
        {
            return Unauthorized("Invalid refresh token");
        }

        storedToken.IsUsed = true;

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == storedToken.UserId);

        var userEntity = new UserEntity(
            user.Id,
            user.Email,
            user.FullName!,
            (Role)(user.Role ?? 0),
            user.IsActive ?? true
        );

        var newAccessToken = _jwtService.GenerateToken(userEntity);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken = newAccessToken
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (storedToken == null)
            return BadRequest();

        storedToken.IsRevoked = true;

        await _context.SaveChangesAsync();

        return Ok();
    }
}