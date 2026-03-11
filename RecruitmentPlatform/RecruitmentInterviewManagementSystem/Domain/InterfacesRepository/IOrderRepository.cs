using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Domain.InterfacesRepository
{
    public interface IOrderRepository
    {
        Task<Order?> GetOrderWithDetailsByIdAsync(Guid orderId);

    }
}
