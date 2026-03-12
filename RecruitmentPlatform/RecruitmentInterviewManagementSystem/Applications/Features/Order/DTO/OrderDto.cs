namespace RecruitmentInterviewManagementSystem.Applications.Features.Order.DTO
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string? OrderCode { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public ServicePackageDto ServicePackage { get; set; } = null!;
    }

    public class ServicePackageDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? DurationDays { get; set; }
        public int? MaxPost { get; set; }
    }
}
