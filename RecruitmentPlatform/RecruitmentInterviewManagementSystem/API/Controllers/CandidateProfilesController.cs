using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateProfilesController : ControllerBase
    {
        private readonly FakeTopcvContext _db;
        private readonly IMinIOCV _minioService;

        public CandidateProfilesController(FakeTopcvContext context, IMinIOCV minioService)
        {
            _db = context;
            _minioService = minioService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<CandidateProfileDto>> GetProfile(Guid userId)
        {
            var profile = await _db.CandidateProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            // --- ĐÃ SỬA: TỰ ĐỘNG TẠO HỒ SƠ NẾU CHƯA CÓ ---
            if (profile == null)
            {
                profile = new CandidateProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                _db.CandidateProfiles.Add(profile);
                await _db.SaveChangesAsync();
            }

            // Sinh link ảnh có thời hạn 1 giờ để trả về cho React (Nếu ứng viên đã có ảnh)
            string? avatarUrl = string.IsNullOrEmpty(profile.AvatarUrl)
                ? null
                : await _minioService.GetUrlImage("avatars", profile.AvatarUrl);

            return Ok(new CandidateProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                DateOfBirth = profile.DateOfBirth,
                Gender = profile.Gender,
                Address = profile.Address,
                ExperienceYears = profile.ExperienceYears,
                CurrentSalary = profile.CurrentSalary,
                DesiredSalary = profile.DesiredSalary,
                JobLevel = profile.JobLevel,
                Summary = profile.Summary,
                AvatarUrl = avatarUrl
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(Guid id, CandidateProfileDto dto)
        {
            if (id != dto.Id) return BadRequest("Lỗi dữ liệu.");

            var profile = await _db.CandidateProfiles.FindAsync(id);
            if (profile == null) return NotFound("Không tìm thấy hồ sơ.");

            profile.DateOfBirth = dto.DateOfBirth;
            profile.Gender = dto.Gender;
            profile.Address = dto.Address;
            profile.ExperienceYears = dto.ExperienceYears;
            profile.CurrentSalary = dto.CurrentSalary;
            profile.DesiredSalary = dto.DesiredSalary;
            profile.JobLevel = dto.JobLevel;
            profile.Summary = dto.Summary;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật hồ sơ thành công!" });
        }

        [HttpPost("{id}/avatar")]
        public async Task<IActionResult> UploadAvatar(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Vui lòng chọn ảnh.");

            var profile = await _db.CandidateProfiles.FindAsync(id);
            if (profile == null) return NotFound("Không tìm thấy hồ sơ.");

            try
            {
                string objectName = await _minioService.UploadAsync(file);

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