namespace RecruitmentInterviewManagementSystem.Applications.Features.Order.DTO
{
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ServicePackageId { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
    }
}
