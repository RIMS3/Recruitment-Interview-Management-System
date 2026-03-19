using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Order.Interface;
using RecruitmentInterviewManagementSystem.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

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

        // ==========================================
        // HÀM HELPER: Lấy UserId thật từ ProfileId trong Token
        // ==========================================
        private async Task<Guid> GetActualUserIdFromTokenAsync()
        {
            var tokenClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(tokenClaimId) || !Guid.TryParse(tokenClaimId, out Guid idFromToken))
                return Guid.Empty;

            // 1. TRƯỜNG HỢP TOKEN LƯU PROFILE_ID (Dành cho nick cũ)
            var profileById = await _context.CandidateProfiles.FirstOrDefaultAsync(c => c.Id == idFromToken);
            if (profileById != null) return profileById.UserId;

            var empById = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.Id == idFromToken);
            if (empById != null) return empById.UserId;

            // 2. TRƯỜNG HỢP TOKEN LƯU USER_ID (Dành cho nick mới)
            var profileByUserId = await _context.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == idFromToken);
            if (profileByUserId != null) return idFromToken;

            var empByUserId = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == idFromToken);
            if (empByUserId != null) return idFromToken;

            // 3. TRƯỜNG HỢP NICK MỚI TINH CHƯA CÓ PROFILE (Chỉ có trong bảng Users)
            // Miễn là token hợp lệ, ta coi idFromToken chính là UserId luôn để họ còn xem được đơn hàng trống.
            return idFromToken;
        }
        // ==========================================

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            Guid actualUserId = await GetActualUserIdFromTokenAsync();

            if (actualUserId == Guid.Empty)
            {
                return BadRequest(new { message = "Lỗi Database: Token đúng nhưng Profile này đã bị xóa hoặc chưa được tạo trong SQL." });
            }
                //return Unauthorized(new { message = "Không tìm thấy thông tin xác thực." });

            var pagedOrders = await _orderService.GetMyOrdersAsync(actualUserId, pageNumber, pageSize);
            return Ok(pagedOrders);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            // 1. Lấy ID trực tiếp từ Token (ID này đang bị lưu nhầm vào bảng Orders lúc tạo đơn)
            var tokenClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(tokenClaimId) || !Guid.TryParse(tokenClaimId, out Guid rawTokenId))
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin xác thực." });
            }

            // 2. Thử tìm đơn hàng bằng cái ID từ Token trước (Chữa cháy cho các đơn cũ bị lưu nhầm ProfileId)
            var order = await _orderService.GetOrderDetailsByIdAsync(id, rawTokenId);

            // 3. Nếu không thấy, lúc này mới thử tìm bằng UserId thật (Cho các đơn lưu chuẩn)
            if (order == null)
            {
                Guid actualUserId = await GetActualUserIdFromTokenAsync();

                if (actualUserId != Guid.Empty && actualUserId != rawTokenId)
                {
                    order = await _orderService.GetOrderDetailsByIdAsync(id, actualUserId);
                }
            }

            // 4. Trả kết quả
            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng này hoặc bạn không có quyền truy cập." });
            }

            return Ok(order);
        }
    }
}