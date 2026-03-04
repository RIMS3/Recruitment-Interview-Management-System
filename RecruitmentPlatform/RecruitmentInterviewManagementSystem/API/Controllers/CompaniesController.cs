using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentInterviewManagementSystem.Applications.Features.Companies.DTO;
using RecruitmentInterviewManagementSystem.Models;
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
        // 1️⃣ Lấy UserId từ token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("id");

        if (userIdClaim == null)
            return Unauthorized("Không tìm thấy userId trong token");

        if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
            return BadRequest("UserId không hợp lệ");

        // 2️⃣ Tìm user trong DB
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound("User không tồn tại");

        // 3️⃣ Kiểm tra role
        if (user.Role != 3)
            return BadRequest("Chỉ Employer mới được tạo công ty");

        // 4️⃣ Kiểm tra đã có company chưa
        var hasProfile = _context.EmployerProfiles
     .Any(e => e.UserId == userId);

        if (hasProfile)
            return BadRequest("Bạn đã tạo công ty rồi");

        // 5️⃣ Tạo Company
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

        // 6️⃣ Tạo EmployerProfile
        var employerProfile = new EmployerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyId = company.Id,
            Position = "Owner"
        };

        _context.EmployerProfiles.Add(employerProfile);

        // 7️⃣ Save DB
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Tạo công ty thành công",
            companyId = company.Id
        });
    }
}