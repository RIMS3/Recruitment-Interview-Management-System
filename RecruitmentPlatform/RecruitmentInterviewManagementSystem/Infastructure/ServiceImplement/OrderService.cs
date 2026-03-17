using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Order.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Order.Interface;
using RecruitmentInterviewManagementSystem.Applications.Features.PagedResult;
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

        public async Task<PagedResult<OrderDto>> GetMyOrdersAsync(Guid userId, int pageNumber, int pageSize)
        {
            var (orders, totalCount) = await _orderRepository.GetOrdersByUserIdAsync(userId, pageNumber, pageSize);

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

            Console.WriteLine("ds");

            return new PagedResult<OrderDto>
            {
                Items = orderDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<OrderDto?> GetOrderDetailsByIdAsync(Guid orderId, Guid userId)
        {
            var order = await _orderRepository.GetOrderDetailsByIdAndUserIdAsync(orderId, userId);
            if (order == null) return null;

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