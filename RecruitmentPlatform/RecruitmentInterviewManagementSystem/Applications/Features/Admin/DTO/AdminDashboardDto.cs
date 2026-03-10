namespace RecruitmentInterviewManagementSystem.Applications.Features.Admin.DTO
{
    public class AdminDashboardDto
    {
        public int TotalCandidates { get; set; }
        public int TotalEmployers { get; set; }
        public int OpenJobs { get; set; }
        public int ClosedJobs { get; set; }
        public int TotalCvs { get; set; }
        public int ApplicationsToday { get; set; }
        public int ApplicationsWeek { get; set; }
        public int ApplicationsMonth { get; set; }
        public List<DailyApplicationStat> WeeklyChart { get; set; } = new();
    }

    public class DailyApplicationStat
    {
        public string Name { get; set; } = string.Empty; // Ví dụ: "Thứ 2" hoặc "01/03"
        public int Applications { get; set; }
    }
}