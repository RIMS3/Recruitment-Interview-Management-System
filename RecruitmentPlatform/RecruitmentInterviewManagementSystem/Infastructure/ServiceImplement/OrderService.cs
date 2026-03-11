using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Order.DTO;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models; // Chứa Entity DbContext của bạn

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly FakeTopcvContext _dbContext;

        public OrderService(IOrderRepository orderRepository, FakeTopcvContext dbContext)
        {
            _orderRepository = orderRepository;
            _dbContext = dbContext;
        }

        public async Task<OrderDetailDto?> GetOrderDetailsAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);

            if (order == null) return null;

            return new OrderDetailDto
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                EmployerId = order.EmployerId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                OrderItems = order.OrderItems.Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    ServicePackageId = item.ServicePackageId,
                    Price = item.Price,
                    Quantity = item.Quantity
                }).ToList()
            };
        }


        public async Task<IEnumerable<OrderDto>> GetOrdersByEmployeeIdAsync(Guid employeeId)
        {
            var orders = await _dbContext.Orders
                .Where(o => o.EmployerId == employeeId) // Lọc cứng theo ID truyền vào
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    EmployerId = o.EmployerId,
                    TotalAmount = o.TotalAmount ?? 0m,
                    Status = o.Status ?? 0,
                    CreatedAt = o.CreatedAt ?? DateTime.UtcNow,
                    PaidAt = o.PaidAt
                })
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders;
        }
    }
}