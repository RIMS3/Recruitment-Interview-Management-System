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

        public async Task<(List<Order> Orders, int TotalCount)> GetOrdersByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            var candidate = await _context.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == userId);

            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ServicePackage)
                .AsQueryable();

            // Tự động nhận diện Role
            if (employer != null)
            {
                query = query.Where(o => o.EmployerId == employer.Id);
            }
            else if (candidate != null)
            {
                query = query.Where(o => o.CandidateId == candidate.Id);
            }
            else
            {
                return (new List<Order>(), 0); // Không có profile thì mảng rỗng
            }

            int totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<Order?> GetOrderDetailsByIdAndUserIdAsync(Guid orderId, Guid userId)
        {
            var employer = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
            var candidate = await _context.CandidateProfiles.FirstOrDefaultAsync(c => c.UserId == userId);

            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ServicePackage)
                .Where(o => o.Id == orderId);

            // Bảo mật: Đảm bảo đơn hàng này đúng là của người đang đăng nhập
            if (employer != null)
            {
                query = query.Where(o => o.EmployerId == employer.Id);
            }
            else if (candidate != null)
            {
                query = query.Where(o => o.CandidateId == candidate.Id);
            }
            else
            {
                return null;
            }

            return await query.FirstOrDefaultAsync();
        }
    }
}
