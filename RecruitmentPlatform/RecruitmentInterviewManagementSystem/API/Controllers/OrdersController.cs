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

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
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

            // Gọi thẳng Service, Service sẽ tự biết tìm Candidate hay Employer
            var pagedOrders = await _orderService.GetMyOrdersAsync(userId, pageNumber, pageSize);

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

            var order = await _orderService.GetOrderDetailsByIdAsync(id, userId);

            if (order == null)
            {
                // Thông báo chung chung để bảo mật, không cho User biết là đơn hàng không tồn tại hay họ không có quyền xem
                return NotFound(new { message = "Không tìm thấy đơn hàng này hoặc bạn không có quyền truy cập." });
            }

            return Ok(order);
        }
    }
}