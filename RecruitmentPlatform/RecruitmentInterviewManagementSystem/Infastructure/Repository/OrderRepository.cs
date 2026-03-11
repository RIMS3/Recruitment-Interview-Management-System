using Google;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FakeTopcvContext _dbContext; // Thay bằng DbContext thực tế của bạn

        public OrderRepository(FakeTopcvContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId)
        {
            return await _dbContext.Orders
                .Include(o => o.OrderItems) // Join với bảng OrderItems
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}
