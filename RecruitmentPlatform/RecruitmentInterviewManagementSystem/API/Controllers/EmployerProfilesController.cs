using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface; // Đã thêm thư viện MinIO
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployerProfilesController : ControllerBase
    {
        private readonly FakeTopcvContext _db;
        private readonly IMinIOCV _minioService; // Khai báo Service MinIO

        // Gộp Database và MinIO vào 1 Constructor duy nhất
        public EmployerProfilesController(FakeTopcvContext context, IMinIOCV minioService)
        {
            _db = context;
            _minioService = minioService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<EmployerProfileDto>> GetProfile(Guid userId)
        {
            var profile = await _db.EmployerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound("Không tìm thấy hồ sơ nhà tuyển dụng.");

            // Sinh link ảnh có thời hạn 1 giờ để trả về cho React (Nếu đã có ảnh)
            string? avatarUrl = string.IsNullOrEmpty(profile.AvatarUrl)
                ? null
                : await _minioService.GetUrlImage("avatars", profile.AvatarUrl);

            return Ok(new EmployerProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                CompanyId = profile.CompanyId,
                Position = profile.Position,
                AvatarUrl = avatarUrl // Trả về link ảnh cho Frontend
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(Guid id, EmployerProfileDto dto)
        {
            if (id != dto.Id) return BadRequest("Lỗi dữ liệu.");

            var profile = await _db.EmployerProfiles.FindAsync(id);
            if (profile == null) return NotFound("Không tìm thấy hồ sơ.");

            profile.Position = dto.Position;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật chức vụ thành công!" });
        }

        // --- TÍNH NĂNG MỚI: TẢI ẢNH ĐẠI DIỆN NHÀ TUYỂN DỤNG ---
        [HttpPost("{id}/avatar")]
        public async Task<IActionResult> UploadAvatar(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Vui lòng chọn ảnh.");

            var profile = await _db.EmployerProfiles.FindAsync(id);

            if (profile == null) return NotFound("Không tìm thấy hồ sơ.");

            try
            {
                if (!string.IsNullOrEmpty(profile.AvatarUrl))
                {
                    await _minioService.DeleteAsync(profile.AvatarUrl, "avatars");
                }

                string objectName = await _minioService.UploadAsync(file, "avatars");

                profile.AvatarUrl = objectName;
                await _db.SaveChangesAsync();

                string displayUrl = await _minioService.GetUrlImage("avatars", objectName);

                return Ok(new { message = "Upload ảnh thành công!", avatarUrl = displayUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}