using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Admin.DTO;
using RecruitmentInterviewManagementSystem.Models;
using System.Globalization;

namespace RecruitmentInterviewManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly FakeTopcvContext _db;

        public AdminController(FakeTopcvContext db)
        {
            _db = db;
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var now = DateTime.Now;
            var startOfToday = now.Date;
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday).Date;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // 1. Thống kê tổng quan dựa trên Role
            var totalCandidates = await _db.Users.CountAsync(u => u.Role == 1);
            var totalEmployers = await _db.Users.CountAsync(u => u.Role == 2);

            // 2. Thống kê Job dựa trên trạng thái (giả định Status 1=Open, 0=Closed)
            var openJobs = await _db.JobPosts.CountAsync(j => j.IsActive == true);
            var closedJobs = await _db.JobPosts.CountAsync(j => j.IsActive == false );

            // 3. Tổng số CV
            var totalCvs = await _db.Cvs.CountAsync();

            // 4. Thống kê lượt ứng tuyển (Applications) theo thời gian
            var appsToday = await _db.Applications.CountAsync(a => a.AppliedAt >= startOfToday);
            var appsWeek = await _db.Applications.CountAsync(a => a.AppliedAt >= startOfWeek);
            var appsMonth = await _db.Applications.CountAsync(a => a.AppliedAt >= startOfMonth);

            // 5. Lấy dữ liệu biểu đồ cho 7 ngày gần nhất
            var weeklyChart = new List<DailyApplicationStat>();
            for (int i = 6; i >= 0; i--)
            {
                var date = now.AddDays(-i).Date;
                var count = await _db.Applications.CountAsync(a => a.AppliedAt.Value.Date == date);

                weeklyChart.Add(new DailyApplicationStat
                {
                    // Lấy tên thứ bằng tiếng Việt
                    Name = date.ToString("dd/MM"),
                    Applications = count
                });
            }

            var result = new AdminDashboardDto
            {
                TotalCandidates = totalCandidates,
                TotalEmployers = totalEmployers,
                OpenJobs = openJobs,
                ClosedJobs = closedJobs,
                TotalCvs = totalCvs,
                ApplicationsToday = appsToday,
                ApplicationsWeek = appsWeek,
                ApplicationsMonth = appsMonth,
                WeeklyChart = weeklyChart
            };

            return Ok(result);
        }
    }
}