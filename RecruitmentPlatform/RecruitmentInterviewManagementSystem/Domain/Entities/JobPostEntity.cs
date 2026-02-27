namespace RecruitmentInterviewManagementSystem.Domain.Entities
{
    public class JobPostEntity
    {
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Requirement { get; set; }

        public string? Benefit { get; set; }

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        public string? Location { get; set; }

        public int? JobType { get; set; }

        public DateTime? ExpireAt { get; set; }

        public bool? IsActive { get; set; }

        public int? ViewCount { get; set; }

        public DateTime? CreatedAt { get; set; }
        public int? Experience { get; set; }
    }
}
