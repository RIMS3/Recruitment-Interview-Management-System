using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentInterviewManagementSystem.Applications.TaiOrXiuFeature;
using System.Security.Claims;

namespace RecruitmentInterviewManagementSystem.API.TaiOrXiuControllers
{
    //[Authorize]
    [Route("api/game")]
    [ApiController]
    public class TaiOrXiuController : ControllerBase
    {
        private readonly IPlaceBet _placeBet;

        public TaiOrXiuController(IPlaceBet placeBet)
        {
            _placeBet = placeBet;
        }

        [HttpPost("bet")]
        public async Task<IActionResult> PlaceBet([FromBody] BetRequest request)
        {

            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be greater than 0");
            }

            var result = await _placeBet.Execute(
                Guid.Parse(request.UserId.ToString()),
                request.BetType,
                request.Amount
            );

            return Ok(new
            {
                success = result
            });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var history = await _placeBet.GetHistory();
            return Ok(history);
        }

  
    }
}