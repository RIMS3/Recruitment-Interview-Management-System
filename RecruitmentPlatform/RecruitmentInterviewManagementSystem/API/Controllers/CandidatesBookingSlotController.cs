using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RecruitmentInterviewManagementSystem.API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatesBookingSlotController : ControllerBase
    {
        public CandidatesBookingSlotController()
        {
        }


        public async Task<IActionResult> Index()
        {

            var userID = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userID != null)
            {

            }

            return Ok();
        }
    }
}
