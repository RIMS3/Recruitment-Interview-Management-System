using Google;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FakeTopcvContext _context; 

        public OrderRepository(FakeTopcvContext context)
        {
            _context = context;
        }

        public async Task<(List<Order> Orders, int TotalCount)> GetOrdersWithDetailsByEmployerIdAsync(Guid employerId, int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ServicePackage)
                .Where(o => o.EmployerId == employerId);

            int totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize) 
                .ToListAsync();
            return (orders, totalCount);
        }

        public async Task<Order?> GetOrderDetailsByIdAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ServicePackage)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}
