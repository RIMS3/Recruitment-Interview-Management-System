using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.DTOs;
using RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Applications.Interface;
using RecruitmentInterviewManagementSystem.Domain.Entities;
using RecruitmentInterviewManagementSystem.Domain.Enums;
using RecruitmentInterviewManagementSystem.Models;
using System.Security.Claims;

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

    // ================= GOOGLE LOGIN =================
    [AllowAnonymous]
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        if (string.IsNullOrEmpty(request.IdToken))
            return BadRequest("IdToken is required");

        var result = await _googleAuthService.LoginAsync(request.IdToken);

        return Ok(new
        {
            accessToken = result.AccessToken,
            email = result.Email,
            fullName = result.FullName,
            userId = result.UserId, // ✅ Trả thêm userId
            role = result.Role      // ✅ Trả thêm role
        });
    }

    // ================= REGISTER =================
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _register.RegisterAsync(request);

        if (result == null)
        {
            return BadRequest(new
            {
                message = "Email đã tồn tại"
            });
        }

        return Ok(result);
    }

    // ================= LOGIN =================
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] RequestLogin request)
    {
        var result = await _login.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized(new
            {
                message = "Email hoặc mật khẩu không đúng"
            });
        }

        return Ok(result);
    }

    // ================= SELECT ROLE =================
    [Authorize]
    [HttpPost("select-role")]
    public async Task<IActionResult> SelectRole([FromBody] SelectRoleRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? User.FindFirst("id")
                 ?? User.FindFirst("sub");

        if (userIdClaim == null)
            return Unauthorized("Token không chứa userId");

        if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
            return BadRequest("UserId trong token không hợp lệ");

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound("User không tồn tại");

        if (user.Role != 0)
            return BadRequest("Role đã được thiết lập trước đó");

        if (request.Role != 2 && request.Role != 3)
            return BadRequest("Role không hợp lệ");

        // SET ROLE
        user.Role = request.Role;

        Guid? candidateId = null;

        // ===== CREATE CANDIDATE PROFILE =====
        if (request.Role == 2)
        {
            var candidate = _context.CandidateProfiles
                .FirstOrDefault(c => c.UserId == user.Id);

            if (candidate == null)
            {
                candidate = new CandidateProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id
                };

                _context.CandidateProfiles.Add(candidate);
            }

            candidateId = candidate.Id;
        }
        await _context.SaveChangesAsync();

        var userEntity = new UserEntity(
            user.Id,
            user.Email,
            user.FullName!,
            (Role)user.Role,
            user.IsActive ?? true
        );

        var newAccessToken = _jwtService.GenerateToken(userEntity);

        return Ok(new
        {
            accessToken = newAccessToken,
            role = user.Role,
            candidateId = candidateId
        });
    }

    // ================= REFRESH TOKEN =================
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
    [AllowAnonymous]
    // ================= LOGOUT =================
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