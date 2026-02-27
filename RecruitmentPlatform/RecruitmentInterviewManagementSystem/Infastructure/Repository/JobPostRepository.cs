using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.JobPost.DTO;
using RecruitmentInterviewManagementSystem.Domain.Entities;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Domain.Enums;
using RecruitmentInterviewManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecruitmentInterviewManagementSystem.Infastructure.Repository
{
    public class JobPostRepository : IJobPostRepository
    {
        private readonly FakeTopcvContext _context;

        public JobPostRepository(FakeTopcvContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobPost>> GetAllAsync()
        {
            return await _context.JobPosts
                                 .Where(x => x.IsActive == true)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<JobPostItemDTO>> GetFilteredJobsAsync(JobPostFilterRequest filter)
        {
            // 1. Khởi tạo truy vấn
            var query = _context.JobPosts.Where(x => x.IsActive == true).AsQueryable();

            // 2. Lọc theo ID
            if (filter.Id.HasValue)
            {
                query = query.Where(x => x.Id == filter.Id.Value);
            }

            // 3. Lọc theo từ khóa
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string searchLower = filter.Search.ToLower();
                query = query.Where(x => x.Title != null && x.Title.ToLower().Contains(searchLower));
            }

            // 4. Lọc theo địa điểm
            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                query = query.Where(x => x.Location != null && x.Location.Contains(filter.Location));
            }

            // 5. Lọc theo lương
            if (filter.MinSalary.HasValue) query = query.Where(x => x.SalaryMin >= filter.MinSalary.Value);
            if (filter.MaxSalary.HasValue) query = query.Where(x => x.SalaryMax <= filter.MaxSalary.Value);

            // 6. Lọc theo JobType (Ép kiểu int để so sánh)
            if (filter.JobType.HasValue)
            {
                query = query.Where(x => (int)x.JobType == filter.JobType.Value);
            }

            // 7. Lọc theo Experience (Dùng == vì Experience là int trong DB)
            if (filter.Experience.HasValue)
            {
                query = query.Where(x => x.Experience == filter.Experience.Value);
            }

            // 8. Phân trang
            int page = filter.PageNumber > 0 ? filter.PageNumber : 1;
            int size = filter.PageSize > 0 ? filter.PageSize : 10;

            // 9. Mapping dữ liệu sang DTO để API có dữ liệu (không bị NULL)
            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
               .Select(x => new JobPostItemDTO
               {
                   IdJobPost = x.Id,
                   Title = x.Title,
                   Location = x.Location,
                   SalaryMin = x.SalaryMin,
                   SalaryMax = x.SalaryMax,
                   ExpireAt = x.ExpireAt,

                   // Đảm bảo lấy giá trị Experience từ Entity x
                   Experience = x.Experience, // Nếu null thì để là 0 năm, hoặc cứ để x.Experience nếu muốn null

                   // Ép kiểu Enum cẩn thận
                   JobType = x.JobType.HasValue ? (JobType)x.JobType.Value : null
               })
                .ToListAsync();
        }

        public async Task<List<string>> GetLocationsAsync()
        {
            return await _context.JobPosts
                .Where(x => !string.IsNullOrEmpty(x.Location))
                .Select(x => x.Location!)
                .Distinct()
                .OrderBy(x => x)    
                .ToListAsync();
        }
    }
}