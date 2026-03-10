using RecruitmentInterviewManagementSystem.Applications.Features.Cvs.DTO;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Cvs.Interface
{
    public interface ICvService
    {
        Task<IEnumerable<CvSummaryDto>> GetCvsByCandidateAsync(Guid candidateId);
        Task<CvDetailDto?> GetCvByIdAsync(Guid id);
        Task<CvDetailDto> CreateCvAsync(CreateCvRequest request);
        Task<CvDetailDto?> UpdateCvAsync(Guid id, UpdateCvRequest request);
        Task<bool> DeleteCvAsync(Guid id);
        Task<(MemoryStream Stream, string ContentType, string FileName)?> DownloadCvAsync(Guid id);

        // Editor Logic
        Task<CvEditorDataDto?> GetEditorDataAsync(Guid cvId);
        Task<CvEditorDataDto?> UpdateEditorDataAsync(Guid cvId, UpdateCvEditorRequest request);
        Task<CvAvatarResponseDto> UpdateAvatarAsync(Guid cvId, IFormFile file);
    }
}
