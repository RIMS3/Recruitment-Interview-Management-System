using RecruitmentInterviewManagementSystem.Domain.Enums;

namespace RecruitmentInterviewManagementSystem.Applications.Features.JobPost.DTO
{
    public class JobPostItemDTO
    {
        public Guid IdJobPost { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public DateTime? ExpireAt { get; set; }

        // Trả về số năm kinh nghiệm (Khớp với kiểu int trong Database của bạn)
        public int? Experience { get; set; }

        // Trả về loại công việc
        public JobType? JobType { get; set; }

        // Thuộc tính hỗ trợ hiển thị tên chữ (ví dụ: "FullTime") trực tiếp nếu cần
        public string? JobTypeName => JobType?.ToString();
    }
}