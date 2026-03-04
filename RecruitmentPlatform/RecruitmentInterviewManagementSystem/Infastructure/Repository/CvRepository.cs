using Microsoft.EntityFrameworkCore;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Infastructure.Repository
{
    public class CvRepository : ICvRepository
    {
        private readonly FakeTopcvContext _db;

        public CvRepository(FakeTopcvContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Cv>> GetCvsByCandidateIdAsync(Guid candidateId)
        {
            return await _db.Cvs
                .Where(cv => cv.CandidateId == candidateId)
                .OrderByDescending(cv => cv.IsDefault)
                .ThenByDescending(cv => cv.UpdatedAt ?? cv.CreatedAt)
                .ToListAsync();
        }

        public async Task<Cv?> GetCvByIdAsync(Guid id)
        {
            return await _db.Cvs.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> CandidateProfileExistsAsync(Guid candidateId)
        {
            return await _db.CandidateProfiles.AnyAsync(c => c.Id == candidateId);
        }

        public async Task AddCvAsync(Cv cv)
        {
            _db.Cvs.Add(cv);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateCvAsync(Cv cv)
        {
            _db.Cvs.Update(cv);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteCvAsync(Cv cv)
        {
            _db.Cvs.Remove(cv);
            await _db.SaveChangesAsync();
        }

        public async Task ResetDefaultCvForCandidateAsync(Guid candidateId, Guid? excludedCvId = null)
        {
            var others = await _db.Cvs
                .Where(x => x.CandidateId == candidateId && (!excludedCvId.HasValue || x.Id != excludedCvId.Value))
                .ToListAsync();

            foreach (var item in others)
            {
                item.IsDefault = false;
                item.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
        }

        public async Task UpdateEditorSectionsAsync(
            Cv cv,
            IEnumerable<CvEducation> educations,
            IEnumerable<CvExperience> experiences,
            IEnumerable<CvProject> projects,
            IEnumerable<CvCertificate> certificates,
            IEnumerable<CvSkill> skills)
        {
            // Xóa data cũ
            var oldEducations = await _db.CvEducations.Where(x => x.Cvid == cv.Id).ToListAsync();
            var oldExperiences = await _db.CvExperiences.Where(x => x.Cvid == cv.Id).ToListAsync();
            var oldProjects = await _db.CvProjects.Where(x => x.Cvid == cv.Id).ToListAsync();
            var oldCertificates = await _db.CvCertificates.Where(x => x.Cvid == cv.Id).ToListAsync();
            var oldSkills = await _db.CvSkills.Where(x => x.Cvid == cv.Id).ToListAsync();

            _db.CvEducations.RemoveRange(oldEducations);
            _db.CvExperiences.RemoveRange(oldExperiences);
            _db.CvProjects.RemoveRange(oldProjects);
            _db.CvCertificates.RemoveRange(oldCertificates);
            _db.CvSkills.RemoveRange(oldSkills);

            // Thêm data mới
            await _db.CvEducations.AddRangeAsync(educations);
            await _db.CvExperiences.AddRangeAsync(experiences);
            await _db.CvProjects.AddRangeAsync(projects);
            await _db.CvCertificates.AddRangeAsync(certificates);
            await _db.CvSkills.AddRangeAsync(skills);

            // Cập nhật info CV
            _db.Cvs.Update(cv);

            await _db.SaveChangesAsync();
        }

        public async Task<List<CvEducation>> GetEducationsAsync(Guid cvId) => await _db.CvEducations.Where(x => x.Cvid == cvId).ToListAsync();
        public async Task<List<CvExperience>> GetExperiencesAsync(Guid cvId) => await _db.CvExperiences.Where(x => x.Cvid == cvId).ToListAsync();
        public async Task<List<CvProject>> GetProjectsAsync(Guid cvId) => await _db.CvProjects.Where(x => x.Cvid == cvId).ToListAsync();
        public async Task<List<CvCertificate>> GetCertificatesAsync(Guid cvId) => await _db.CvCertificates.Where(x => x.Cvid == cvId).ToListAsync();
        public async Task<List<CvSkill>> GetSkillsAsync(Guid cvId) => await _db.CvSkills.Where(x => x.Cvid == cvId).ToListAsync();
    }
}
