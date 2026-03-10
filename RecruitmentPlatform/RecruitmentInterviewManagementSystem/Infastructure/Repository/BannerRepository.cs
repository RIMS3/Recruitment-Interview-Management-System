using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Infastructure.Models;
using RecruitmentInterviewManagementSystem.Models;
using System;

namespace RecruitmentInterviewManagementSystem.Infastructure.Repository
{
    public class BannerRepository : IBannerRepository
    {
        private readonly FakeTopcvContext _context;

        public BannerRepository(FakeTopcvContext context)
        {
            _context = context;
        }

        public async Task<List<Banner>> GetAllAsync() => await _context.Banners.ToListAsync();
        public async Task<Banner?> GetByIdAsync(int id) => await _context.Banners.FindAsync(id);
        public async Task AddAsync(Banner banner) { _context.Banners.Add(banner); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(Banner banner) { _context.Banners.Update(banner); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(Banner banner) { _context.Banners.Remove(banner); await _context.SaveChangesAsync(); }
    }
}
