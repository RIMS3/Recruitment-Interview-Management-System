using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecruitmentInterviewManagementSystem.Applications.Features.BookingInterviewSlot.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.BookingInterviewSlot.Interfaces;

namespace RecruitmentInterviewManagementSystem.API.Controllers
{
    
    [Route("api/interview")]
    [ApiController]
    public class InterviewSlotController : ControllerBase
    {
        private readonly IRemoveInterviewSlot _removeSlot;
        private readonly IViewListSlotInterviewRoleEmployer _interview;
        private readonly ICreateNewInterviewSlot _createSlot;
        private readonly IUpdateInterviewSlot _udpate;

        public InterviewSlotController(IRemoveInterviewSlot removeInterviewSlot, IViewListSlotInterviewRoleEmployer viewListSlotInterviewRoleEmployer, ICreateNewInterviewSlot createNewInterviewSlot, IUpdateInterviewSlot updateInterviewSlot)
        {
            _removeSlot = removeInterviewSlot;
            _interview = viewListSlotInterviewRoleEmployer;
            _createSlot = createNewInterviewSlot;
            _udpate = updateInterviewSlot;
        }



        [HttpGet("slots")]
        public async Task<IActionResult> Index([FromQuery] RequestViewInterviewSlots request)
        {
            var interviewSlot = await _interview.Execute(request);

            return Ok(interviewSlot);
        }


        [HttpPost("slots")]
        public async Task<IActionResult> CreateSlot([FromBody] RequestCreateNewInterviewSlotDTO request)
        {
            var NewSlot = await _createSlot.Execute(request);

            return Ok(NewSlot);
        }

        [HttpPut("slots")]
        public async Task<IActionResult> UpdateSlot([FromBody] RequestUpdateInterviewSlots request)
        {
            var UpdateSlot = await _udpate.Execute(request);

            return Ok(UpdateSlot);
        }

        [HttpDelete("slots")]
        public async Task<IActionResult> Deletelot([FromBody] RequestRemoveInterviewSlot request)
        {
            var UpdateSlot = await _removeSlot.Execute(request);

            return Ok(UpdateSlot);
        }
    }
}
