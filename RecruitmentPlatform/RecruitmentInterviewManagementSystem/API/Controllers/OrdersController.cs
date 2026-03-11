using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;
using System.Security.Claims;

namespace RecruitmentInterviewManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly FakeTopcvContext _context;

        public OrdersController(IOrderService orderService, FakeTopcvContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        [HttpGet]
        [Authorize] // Bắt buộc phải có token
        public async Task<IActionResult> GetMyOrders()
        {
            // 1. Lấy UserId từ Token (Giống hệt cách bạn vừa gửi)
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("id")?.Value
                            ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin xác thực UserId hợp lệ trong Token." });
            }

            // 2. Tìm Hồ sơ Employer/Employee tương ứng với UserId này trong DB
            // (Lưu ý: Thay EmployerProfiles bằng tên bảng thực tế của bạn)
            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);

            if (employer == null)
            {
                // Nếu tài khoản này chưa tạo profile nhà tuyển dụng, trả về mảng rỗng hoặc báo lỗi
                return NotFound(new { message = "Không tìm thấy hồ sơ Employer cho tài khoản này." });
            }

            // 3. Dùng chính cái employer.Id lấy được từ DB để lấy danh sách đơn hàng
            var orders = await _orderService.GetOrdersByEmployeeIdAsync(employer.Id);

            if (orders == null || !orders.Any())
            {
                return Ok(new List<object>()); // Chưa có đơn hàng nào
            }

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(Guid id)
        {
            var result = await _orderService.GetOrderDetailsAsync(id);

            if (result == null)
            {
                return NotFound(new { message = $"Không tìm thấy Order với Id: {id}" });
            }

            return Ok(result);
        }
    }
}