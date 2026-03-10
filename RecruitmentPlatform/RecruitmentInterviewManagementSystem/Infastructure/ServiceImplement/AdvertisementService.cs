using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Applications.Features.Advertisement.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Advertisement.Interface;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Infastructure.Models;
using RecruitmentInterviewManagementSystem.Models;
using System;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class AdvertisementService : IAdvertisementService
    {
        private readonly FakeTopcvContext _context;
        private readonly IMinIOCV _minIOService; // Sử dụng IMinIOCV

        // Tên bucket dành cho quảng cáo
        private readonly string _bucketName = "advertisements";

        public AdvertisementService(FakeTopcvContext context, IMinIOCV minIOService)
        {
            _context = context;
            _minIOService = minIOService;
        }

        public async Task<IEnumerable<AdvertisementDTO>> GetAllAsync()
        {
            var ads = await _context.Advertisements.ToListAsync();
            var dtos = new List<AdvertisementDTO>();

            foreach (var ad in ads)
            {
                // Dùng hàm GetUrlImage của bạn
                string? imageUrl = await _minIOService.GetUrlImage(_bucketName, ad.ImageName);

                dtos.Add(new AdvertisementDTO
                {
                    Id = ad.Id,
                    Title = ad.Title,
                    ImageUrl = imageUrl ?? "",
                    Duration = ad.Duration,
                    LinkUrl = ad.LinkUrl,
                    IsPopup = ad.IsPopup
                });
            }
            return dtos;
        }

        public async Task<AdvertisementDTO> GetByIdAsync(int id)
        {
            var ad = await _context.Advertisements.FindAsync(id);
            if (ad == null) return null;

            // Dùng hàm GetUrlImage của bạn
            string? imageUrl = await _minIOService.GetUrlImage(_bucketName, ad.ImageName);

            return new AdvertisementDTO
            {
                Id = ad.Id,
                Title = ad.Title,
                ImageUrl = imageUrl ?? "",
                Duration = ad.Duration,
                LinkUrl = ad.LinkUrl,
                IsPopup = ad.IsPopup
            };
        }

        public async Task<AdvertisementDTO> CreateAsync(CreateAdvertisementDTO dto)
        {
            // 1. Dùng hàm UploadAsync của bạn (Hàm này thường trả về string là tên file/objectName)
            string fileName = await _minIOService.UploadAsync(dto.ImageFile, _bucketName);

            // 2. Lưu vào DB
            var ad = new Advertisement
            {
                Title = dto.Title,
                ImageName = fileName,
                Duration = dto.Duration,
                LinkUrl = dto.LinkUrl,
                IsPopup = dto.IsPopup
            };

            _context.Advertisements.Add(ad);
            await _context.SaveChangesAsync();

            // 3. Lấy URL hiển thị
            string? imageUrl = await _minIOService.GetUrlImage(_bucketName, fileName);

            return new AdvertisementDTO
            {
                Id = ad.Id,
                Title = ad.Title,
                ImageUrl = imageUrl ?? "",
                Duration = ad.Duration,
                LinkUrl = ad.LinkUrl,
                IsPopup = ad.IsPopup
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateAdvertisementDTO dto)
        {
            var ad = await _context.Advertisements.FindAsync(id);
            if (ad == null) return false;

            ad.Title = dto.Title;
            ad.Duration = dto.Duration;
            ad.LinkUrl = dto.LinkUrl;
            ad.IsPopup = dto.IsPopup;

            // Nếu có cập nhật ảnh mới
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                // 1. Xóa ảnh cũ bằng hàm DeleteAsync của bạn (Chú ý thứ tự tham số: objectName, bucketName)
                await _minIOService.DeleteAsync(ad.ImageName, _bucketName);

                // 2. Up ảnh mới lên
                string newFileName = await _minIOService.UploadAsync(dto.ImageFile, _bucketName);
                ad.ImageName = newFileName;
            }

            _context.Advertisements.Update(ad);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ad = await _context.Advertisements.FindAsync(id);
            if (ad == null) return false;

            // Xóa ảnh trên MinIO bằng hàm của bạn
            await _minIOService.DeleteAsync(ad.ImageName, _bucketName);

            _context.Advertisements.Remove(ad);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
