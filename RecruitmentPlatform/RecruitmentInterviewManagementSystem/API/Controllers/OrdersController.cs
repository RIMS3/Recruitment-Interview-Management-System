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
            if (string.IsNullOrEmpty(tokenClaimId) || !Guid.TryParse(tokenClaimId, out Guid profileId))
                return Guid.Empty;

            var isCandidate = await _context.CandidateProfiles.AnyAsync(c => c.Id == profileId);
            if (isCandidate)
            {
                return await _context.CandidateProfiles
                    .Where(c => c.Id == profileId).Select(c => c.UserId).FirstOrDefaultAsync();
            }

            return await _context.EmployerProfiles
                .Where(e => e.Id == profileId).Select(e => e.UserId).FirstOrDefaultAsync();
        }
        // ==========================================

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            Guid actualUserId = await GetActualUserIdFromTokenAsync();

            if (actualUserId == Guid.Empty)
                return Unauthorized(new { message = "Không tìm thấy thông tin xác thực." });

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