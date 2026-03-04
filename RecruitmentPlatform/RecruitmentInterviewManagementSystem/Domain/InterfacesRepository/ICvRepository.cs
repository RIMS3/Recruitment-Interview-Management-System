using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Domain.InterfacesRepository
{
    public interface ICvRepository
    {
        Task<IEnumerable<Cv>> GetCvsByCandidateIdAsync(Guid candidateId);
        Task<Cv?> GetCvByIdAsync(Guid id);
        Task<bool> CandidateProfileExistsAsync(Guid candidateId);

        Task AddCvAsync(Cv cv);
        Task UpdateCvAsync(Cv cv);
        Task DeleteCvAsync(Cv cv);
        Task ResetDefaultCvForCandidateAsync(Guid candidateId, Guid? excludedCvId = null);

        // Dành cho Editor
        Task UpdateEditorSectionsAsync(
            Cv cv,
            IEnumerable<CvEducation> educations,
            IEnumerable<CvExperience> experiences,
            IEnumerable<CvProject> projects,
            IEnumerable<CvCertificate> certificates,
            IEnumerable<CvSkill> skills);

        Task<List<CvEducation>> GetEducationsAsync(Guid cvId);
        Task<List<CvExperience>> GetExperiencesAsync(Guid cvId);
        Task<List<CvProject>> GetProjectsAsync(Guid cvId);
        Task<List<CvCertificate>> GetCertificatesAsync(Guid cvId);
        Task<List<CvSkill>> GetSkillsAsync(Guid cvId);
    }
}
