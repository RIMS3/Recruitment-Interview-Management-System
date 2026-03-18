using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecruitmentInterviewManagementSystem.API.DTOs;
using RecruitmentInterviewManagementSystem.Applications.Features.Schedule;

namespace RecruitmentInterviewManagementSystem.API.ScheduleCandidate
{
    [Route("api/schedule")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IViewListScheduleForCandidate _viewSchedule;
        private readonly IBookInterviewSlotForCandidate _bookingSlot;

        public ScheduleController(IViewListScheduleForCandidate viewListScheduleForCandidate, IBookInterviewSlotForCandidate bookInterviewSlotForCandidate)
        {
            _viewSchedule = viewListScheduleForCandidate;
            _bookingSlot = bookInterviewSlotForCandidate;
        }

        // candidate view slot 
        [HttpGet("{Token}")]
        public async Task<IActionResult> ViewSchedule([FromRoute] Guid Token)
        {
            var schedules = await _viewSchedule.Execute(Token.ToString());

            return Ok(schedules);
        }


        [HttpPost("book")]
        public async Task<IActionResult> BookingSlot([FromBody] BookInterviewRequestDto request)
        {
            var result = await _bookingSlot.ExecuteAsync(request.Token, request.SlotId);
            return Ok(new {status = result.IsSuccess, message = result.Message});
        }

    }
}
