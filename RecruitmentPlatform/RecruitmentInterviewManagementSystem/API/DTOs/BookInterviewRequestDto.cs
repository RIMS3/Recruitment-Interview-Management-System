namespace RecruitmentInterviewManagementSystem.API.DTOs
{
    public class BookInterviewRequestDto
    {
        public string Token { get; set; } = null!;
        public Guid SlotId { get; set; }
    }
}
