using RecruitmentInterviewManagementSystem.Applications.Features.Order.DTO;

namespace RecruitmentInterviewManagementSystem.Domain.InterfacesRepository
{
    public interface IOrderService
    {
        Task<OrderDetailDto?> GetOrderDetailsAsync(Guid orderId);
        Task<IEnumerable<OrderDto>> GetOrdersByEmployeeIdAsync(Guid employeeId);
    }
}
