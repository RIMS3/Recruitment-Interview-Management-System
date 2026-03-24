namespace RecruitmentInterviewManagementSystem.Models.DTOs
{
    public class ViewListJobApplyDTO
    {
        public Guid ApplicationId { get; set; }
        public Guid JobId { get; set; }
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public DateTime? AppliedAt { get; set; }
        public int? Status { get; set; }
        
        // Thuộc tính mới để điều khiển việc hiển thị/vô hiệu hóa nút Hủy
        // Chỉ cho phép hủy khi Status là Pending (0)
        public bool CanUnapply => Status == 0;
    }
}