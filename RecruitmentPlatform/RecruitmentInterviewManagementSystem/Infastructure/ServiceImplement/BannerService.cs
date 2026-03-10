using RecruitmentInterviewManagementSystem.Applications.Features.Banner.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Banner.Interface;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Infastructure.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class BannerService : IBannerService
    {
        private readonly IMinIOCV _minio;
        private readonly IBannerRepository _repo; // Đã dùng Interface
        private const string BUCKET = "banners";

        public BannerService(IMinIOCV minio, IBannerRepository repo)
        {
            _minio = minio;
            _repo = repo;
        }

        public async Task<IEnumerable<BannerDTO>> GetAllBannersAsync()
        {
            var banners = await _repo.GetAllAsync();
            var dtos = new List<BannerDTO>();
            foreach (var b in banners)
            {
                dtos.Add(new BannerDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    ImageUrl = await _minio.GetUrlImage(BUCKET, b.ImageName)
                });
            }
            return dtos;
        }

        public async Task CreateAsync(CreateBannerDTO dto)
        {
            var fileName = await _minio.UploadAsync(dto.ImageFile, BUCKET);
            await _repo.AddAsync(new Banner { Title = dto.Title, ImageName = fileName });
        }

        public async Task UpdateAsync(int id, UpdateBannerDTO dto)
        {
            var banner = await _repo.GetByIdAsync(id);
            if (banner == null) return;

            banner.Title = dto.Title;
            if (dto.ImageFile != null)
            {
                await _minio.DeleteAsync(banner.ImageName, BUCKET);
                banner.ImageName = await _minio.UploadAsync(dto.ImageFile, BUCKET);
            }
            await _repo.UpdateAsync(banner);
        }

        public async Task DeleteAsync(int id)
        {
            var banner = await _repo.GetByIdAsync(id);
            if (banner != null)
            {
                await _minio.DeleteAsync(banner.ImageName, BUCKET);
                await _repo.DeleteAsync(banner);
            }
        }
    }
}
