namespace RecruitmentInterviewManagementSystem.Models.DTOs
{
    public class ViewListJobApplyDTO
    {
        public Guid ApplicationId { get; set; }
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public DateTime? AppliedAt { get; set; }
        public int? Status { get; set; }
    }
}