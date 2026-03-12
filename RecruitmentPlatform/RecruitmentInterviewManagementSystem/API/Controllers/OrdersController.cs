using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Order.Interface;
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
        [Authorize]
        public async Task<IActionResult> GetMyOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("id")?.Value
                            ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin xác thực UserId hợp lệ trong Token." });
            }

            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);

            if (employer == null)
            {
                return NotFound(new { message = "Không tìm thấy hồ sơ Employer cho tài khoản này." });
            }

            var pagedOrders = await _orderService.GetOrdersByEmployeeIdAsync(employer.Id, pageNumber, pageSize);

            return Ok(pagedOrders);
        }

        [HttpGet("{id}")] 
        [Authorize] 
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("id")?.Value
                            ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin xác thực." });
            }

            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employer == null)
            {
                return NotFound(new { message = "Không tìm thấy hồ sơ Employer." });
            }

            var order = await _orderService.GetOrderDetailsByIdAsync(id);

            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng này." });
            }

            var dbOrderCheck = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (dbOrderCheck != null && dbOrderCheck.EmployerId != employer.Id)
            {
                return Forbid(); 
            }

            return Ok(order);
        }
    }
}