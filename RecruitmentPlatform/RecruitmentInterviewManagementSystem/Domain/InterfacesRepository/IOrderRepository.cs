using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Domain.InterfacesRepository
{
    public interface IOrderRepository
    {
        Task<(List<Order> Orders, int TotalCount)> GetOrdersByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<Order?> GetOrderDetailsByIdAndUserIdAsync(Guid orderId, Guid userId);
    }
}
