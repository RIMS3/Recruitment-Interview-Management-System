using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Order.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Order.Interface;
using RecruitmentInterviewManagementSystem.Applications.Features.PagedResult;


// THÊM DÒNG NÀY ĐỂ GỌI DTO CŨ VÀO SERVICE:
using RecruitmentInterviewManagementSystem.Applications.Features.ServicePackage.DTO;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<PagedResult<OrderDto>> GetOrdersByEmployeeIdAsync(Guid employerId, int pageNumber, int pageSize)
        {
            // Nhận dữ liệu từ Repository
            var (orders, totalCount) = await _orderRepository.GetOrdersWithDetailsByEmployerIdAsync(employerId, pageNumber, pageSize);

            // Map Entity sang DTO (Đoạn này giữ nguyên y hệt như code cũ của bạn)
            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    ServicePackage = oi.ServicePackage != null ? new Applications.Features.Order.DTO.ServicePackageDto
                    {
                        Id = oi.ServicePackage.Id,
                        Name = oi.ServicePackage.Name,
                        Description = oi.ServicePackage.Description,
                        Price = oi.ServicePackage.Price,
                        DurationDays = oi.ServicePackage.DurationDays,
                        MaxPost = oi.ServicePackage.MaxPost
                    } : null
                }).ToList()
            }).ToList();

            // Gói vào PagedResult
            return new PagedResult<OrderDto>
            {
                Items = orderDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<OrderDto?> GetOrderDetailsByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderDetailsByIdAsync(orderId);

            if (order == null) return null;

            // Tái sử dụng logic Map sang DTO
            return new OrderDto
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    ServicePackage = oi.ServicePackage != null ? new Applications.Features.Order.DTO.ServicePackageDto
                    {
                        Id = oi.ServicePackage.Id,
                        Name = oi.ServicePackage.Name,
                        Description = oi.ServicePackage.Description,
                        Price = oi.ServicePackage.Price,
                        DurationDays = oi.ServicePackage.DurationDays,
                        MaxPost = oi.ServicePackage.MaxPost
                    } : null
                }).ToList()
            };
        }

    }
}