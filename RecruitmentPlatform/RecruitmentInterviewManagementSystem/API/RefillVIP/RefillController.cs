using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecruitmentInterviewManagementSystem.Applications.Payments.DTO;
using RecruitmentInterviewManagementSystem.Applications.Payments.Interface;
using System.Threading.Tasks;

namespace RecruitmentInterviewManagementSystem.API.RefillVIP
{
    [Route("api/refill")]
    [ApiController]
    public class RefillController : ControllerBase
    {
        private readonly IRefill _refill;

        public RefillController(IRefill refill)
        {
            _refill = refill;
        }


        [HttpPost]
        public async Task<IActionResult> RefillVIP([FromBody] RefillDTO request)
        {
            var result = await _refill.Execute(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("{idUser}")]
        public async Task<IActionResult> GetBalance([FromRoute] Guid idUser)
        {
            var balance = await _refill.Execute(idUser);
            return Ok(balance);
        }

        [HttpPost("gift-code")]
        public async Task<IActionResult> GiftCodeBeginer([FromBody] CodeBeginer code)
        {
            var result = await _refill.GiftCodeBeginer(code);
            if (result)
            {
                return Ok(new { Message = "+ 500k Chúc người AE may mắn" });
            }
            else
            {
                return BadRequest(new { Message = "Mã không hợp lệ hoặc đã sử dụng " });
            }
        }
    }
}
