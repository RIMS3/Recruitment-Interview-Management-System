using RecruitmentInterviewManagementSystem.Applications.Features.Order.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.PagedResult;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Order.Interface
{
    public interface IOrderService
    {
        Task<PagedResult<OrderDto>> GetMyOrdersAsync(Guid userId, int pageNumber, int pageSize);
        Task<OrderDto?> GetOrderDetailsByIdAsync(Guid orderId, Guid userId);
    }
}
