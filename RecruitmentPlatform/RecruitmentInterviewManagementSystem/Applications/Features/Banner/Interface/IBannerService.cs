using RecruitmentInterviewManagementSystem.Applications.Features.Banner.DTO;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Banner.Interface
{
        public interface IBannerService
        {
            Task<IEnumerable<BannerDTO>> GetAllBannersAsync();
            Task CreateAsync(CreateBannerDTO dto);
            Task UpdateAsync(int id, UpdateBannerDTO dto);
            Task DeleteAsync(int id);
        }
}
