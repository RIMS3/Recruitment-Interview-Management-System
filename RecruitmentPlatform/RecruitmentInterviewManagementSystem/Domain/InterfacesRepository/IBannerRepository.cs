using RecruitmentInterviewManagementSystem.Infastructure.Models;

namespace RecruitmentInterviewManagementSystem.Domain.InterfacesRepository
{
    public interface IBannerRepository
    {
        Task<List<Banner>> GetAllAsync();
        Task<Banner?> GetByIdAsync(int id);
        Task AddAsync(Banner banner);
        Task UpdateAsync(Banner banner);
        Task DeleteAsync(Banner banner);
    }
}
