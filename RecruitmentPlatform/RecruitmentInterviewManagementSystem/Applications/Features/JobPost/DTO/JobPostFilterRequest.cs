namespace RecruitmentInterviewManagementSystem.Applications.Features.JobPost.DTO
{
    public class JobPostFilterRequest
    {
        public Guid? Id { get; set; }           // THÊM: Để lọc chính xác 1 ID
        public string? Search { get; set; }     // Ánh xạ với từ khóa tìm kiếm
        public string? Location { get; set; }   // Ánh xạ với địa điểm
        public decimal? MinSalary { get; set; } // Lương tối thiểu
        public decimal? MaxSalary { get; set; } // Lương tối đa
        public int? JobType { get; set; }       // Loại công việc (1: Full-time, v.v.)
        public int? Experience { get; set; }
        public int PageNumber { get; set; } = 1; // Mặc định trang 1
        public int PageSize { get; set; } = 10; // Mặc định 10 job/trang

    }
}
