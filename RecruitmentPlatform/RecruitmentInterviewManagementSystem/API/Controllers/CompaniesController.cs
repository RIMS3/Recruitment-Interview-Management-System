using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Companies.DTO;
using RecruitmentInterviewManagementSystem.Models;
using RecruitmentInterviewManagementSystem.Domain.Entities; // Đảm bảo đúng namespace cho Company/EmployerProfile
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class CompaniesController : ControllerBase
{
    private readonly FakeTopcvContext _context;

    public CompaniesController(FakeTopcvContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request)
    {
        // 1. Lấy UserId từ token (NameIdentifier là chuẩn nhất cho Guid)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");

        if (userIdClaim == null)
            return Unauthorized("Không tìm thấy userId trong token");

        if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
            return BadRequest("UserId không hợp lệ");

        // 2. Tìm user trong DB
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound("User không tồn tại");

        // 3. Kiểm tra role (Role 3 là Employer)
        if (user.Role != 3)
            return BadRequest("Chỉ Employer mới được tạo công ty");

        // 4. Kiểm tra xem đã có Profile chưa
        var hasProfile = await _context.EmployerProfiles.AnyAsync(e => e.UserId == userId);
        if (hasProfile)
            return BadRequest("Bạn đã tạo hồ sơ nhà tuyển dụng/công ty rồi");

        // Dùng Transaction để đảm bảo nếu tạo Company lỗi thì Profile cũng không được tạo
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 5. Tạo Company
            var company = new Company
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                TaxCode = request.TaxCode,
                Address = request.Address,
                Website = request.Website,
                Description = request.Description,
                LogoUrl = request.LogoUrl,
                CreatedAt = DateTime.Now
            };

            _context.Companies.Add(company);

            // 6. Tạo EmployerProfile (Liên kết trực tiếp với Company vừa tạo)
            var employerProfile = new EmployerProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CompanyId = company.Id,
                Position = "Owner"
            };

            _context.EmployerProfiles.Add(employerProfile);

            // 7. Lưu tất cả
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Tạo công ty thành công",
                companyId = company.Id,
                employerId = employerProfile.Id
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
        }
    }
}