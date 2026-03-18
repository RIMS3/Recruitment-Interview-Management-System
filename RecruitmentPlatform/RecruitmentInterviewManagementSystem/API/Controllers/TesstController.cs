using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.ChangePassword;
using RecruitmentInterviewManagementSystem.Models;
using System.Runtime.InteropServices;

namespace RecruitmentInterviewManagementSystem.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class TesstController : ControllerBase
    {
        private readonly FakeTopcvContext _db;
        private readonly IResetPassword _resetpass;
        private readonly IChangePasswrod _changePassword;

        public TesstController(FakeTopcvContext fakeTopcvContext, IResetPassword resetPassword, IChangePasswrod changePasswrod)
        {
            _db = fakeTopcvContext;
            _resetpass = resetPassword;
            _changePassword = changePasswrod;
        }

        [HttpPost("password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _resetpass.Execute(request);
            return Ok(result);
        }

        [HttpPatch("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _changePassword.Execute(request);
            return Ok(result);
        }
    }
}