namespace RecruitmentInterviewManagementSystem.Applications.Features.Auth.DTO
{
    public class DashboardDataDto
    {
        // KPI Cards
        public int TotalCandidates { get; set; }
        public int TotalEmployers { get; set; }
        public int OpenJobs { get; set; }
        public int ClosedJobs { get; set; }
        public int TotalCVs { get; set; }

        public int AppliesToday { get; set; }
        public int AppliesThisWeek { get; set; }
        public int AppliesThisMonth { get; set; }

        // Dữ liệu cho Biểu đồ đường (Lượt ứng tuyển 7 ngày qua)
        public List<ChartData> ApplicationTrends { get; set; }
    }

    public class ChartData
    {
        public string Date { get; set; }
        public int Applies { get; set; }
    }}
