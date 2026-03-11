namespace RecruitmentInterviewManagementSystem.Applications.Features.Order.DTO
{
    public class OrderDetailDto
    {
        public Guid Id { get; set; }
        public string? OrderCode { get; set; }
        public Guid EmployerId { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }
}
