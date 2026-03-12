using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Domain.InterfacesRepository
{
    public interface IOrderRepository
    {
        Task<(List<Order> Orders, int TotalCount)> GetOrdersWithDetailsByEmployerIdAsync(Guid employerId, int pageNumber, int pageSize);
        Task<Order?> GetOrderDetailsByIdAsync(Guid orderId);
    }
}
