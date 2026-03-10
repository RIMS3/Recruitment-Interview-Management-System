using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecruitmentInterviewManagementSystem.Applications.Features.Banner.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Banner.Interface;

namespace RecruitmentInterviewManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/banners")]
    public class BannersController : ControllerBase
    {
        private readonly IBannerService _service;

        public BannersController(IBannerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _service.GetAllBannersAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateBannerDTO dto)
        {
            if (dto.ImageFile == null) return BadRequest("Vui lòng chọn ảnh.");

            await _service.CreateAsync(dto);
            return Ok(new { message = "Thêm banner thành công" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateBannerDTO dto)
        {
            await _service.UpdateAsync(id, dto);
            return Ok(new { message = "Cập nhật banner thành công" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(new { message = "Xóa banner thành công" });
        }
    }
}
