using RecruitmentInterviewManagementSystem.Applications.Features.Cvs.DTO;
using RecruitmentInterviewManagementSystem.Applications.Features.Cvs.Interface;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;
using RecruitmentInterviewManagementSystem.Domain.InterfacesRepository;
using RecruitmentInterviewManagementSystem.Models;

namespace RecruitmentInterviewManagementSystem.Applications.Features.Cvs.Services
{
    public class CvService : ICvService
    {
        private readonly ICvRepository _cvRepository;
        private readonly IMinIOCV _minioService;
        private readonly IConfiguration _configuration;

        public CvService(ICvRepository cvRepository, IMinIOCV minioService, IConfiguration configuration)
        {
            _cvRepository = cvRepository;
            _minioService = minioService;
            _configuration = configuration;
        }

        public async Task<IEnumerable<CvSummaryDto>> GetCvsByCandidateAsync(Guid candidateId)
        {
            var cvs = await _cvRepository.GetCvsByCandidateIdAsync(candidateId);
            return cvs.Select(cv => new CvSummaryDto
            {
                Id = cv.Id,
                CandidateId = cv.CandidateId,
                FullName = cv.FullName,
                Position = cv.Position,
                ExperienceYears = cv.ExperienceYears,
                IsDefault = cv.IsDefault ?? false,
                UpdatedAt = cv.UpdatedAt
            });
        }

        public async Task<CvDetailDto?> GetCvByIdAsync(Guid id)
        {
            var cv = await _cvRepository.GetCvByIdAsync(id);
            if (cv == null) return null;
            return await MapToDetailDto(cv);
        }

        public async Task<CvDetailDto> CreateCvAsync(CreateCvRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("Họ tên không được để trống.");

            var candidateExists = await _cvRepository.CandidateProfileExistsAsync(request.CandidateId);
            if (!candidateExists)
                throw new KeyNotFoundException("Không tìm thấy hồ sơ ứng viên.");

            var cv = new Cv
            {
                Id = Guid.NewGuid(),
                CandidateId = request.CandidateId,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Birthday = request.Birthday,
                Gender = request.Gender,
                Nationality = request.Nationality,
                Position = request.Position,
                ExperienceYears = request.ExperienceYears,
                EducationSummary = request.EducationSummary,
                Field = request.Field,
                CurrentSalary = request.CurrentSalary,
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (request.File != null && request.File.Length > 0)
            {
                var bucketName = _configuration["Minio:CvBucket"] ?? "cvs";
                var objectName = await _minioService.UploadAsync(request.File, bucketName);
                cv.FileName = request.File.FileName;
                cv.MimeType = request.File.ContentType;
                cv.FileUrl = objectName;
            }

            if (cv.IsDefault == true)
            {
                await _cvRepository.ResetDefaultCvForCandidateAsync(request.CandidateId);
            }

            await _cvRepository.AddCvAsync(cv);
            return await MapToDetailDto(cv);
        }

        public async Task<CvDetailDto?> UpdateCvAsync(Guid id, UpdateCvRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("Họ tên không được để trống.");

            var cv = await _cvRepository.GetCvByIdAsync(id);
            if (cv == null) return null;

            cv.FullName = request.FullName;
            cv.Email = request.Email;
            cv.PhoneNumber = request.PhoneNumber;
            cv.Address = request.Address;
            cv.Birthday = request.Birthday;
            cv.Gender = request.Gender;
            cv.Nationality = request.Nationality;
            cv.Position = request.Position;
            cv.ExperienceYears = request.ExperienceYears;
            cv.EducationSummary = request.EducationSummary;
            cv.Field = request.Field;
            cv.CurrentSalary = request.CurrentSalary;
            cv.IsDefault = request.IsDefault;
            cv.UpdatedAt = DateTime.UtcNow;

            if (request.File != null && request.File.Length > 0)
            {
                var bucketName = _configuration["Minio:CvBucket"] ?? "cvs";

                if (!string.IsNullOrWhiteSpace(cv.FileUrl))
                {
                    await _minioService.DeleteAsync(cv.FileUrl, bucketName);
                }

                var objectName = await _minioService.UploadAsync(request.File, bucketName);
                cv.FileName = request.File.FileName;
                cv.MimeType = request.File.ContentType;
                cv.FileUrl = objectName;
            }

            if (cv.IsDefault == true)
            {
                await _cvRepository.ResetDefaultCvForCandidateAsync(cv.CandidateId, cv.Id);
            }

            await _cvRepository.UpdateCvAsync(cv);
            return await MapToDetailDto(cv);
        }

        public async Task<bool> DeleteCvAsync(Guid id)
        {
            var cv = await _cvRepository.GetCvByIdAsync(id);
            if (cv == null) return false;

            if (!string.IsNullOrWhiteSpace(cv.FileUrl))
            {
                var bucketName = _configuration["Minio:CvBucket"] ?? "cvs";
                await _minioService.DeleteAsync(cv.FileUrl, bucketName);
            }

            await _cvRepository.DeleteCvAsync(cv);
            return true;
        }

        public async Task<(MemoryStream Stream, string ContentType, string FileName)?> DownloadCvAsync(Guid id)
        {
            var cv = await _cvRepository.GetCvByIdAsync(id);
            if (cv == null || string.IsNullOrEmpty(cv.FileUrl)) return null;

            var bucketName = _configuration["Minio:CvBucket"] ?? "cvs";
            var memoryStream = new MemoryStream();
            await _minioService.GetObjectStreamAsync(bucketName, cv.FileUrl, stream =>
            {
                stream.CopyTo(memoryStream);
            });
            memoryStream.Position = 0;

            return (memoryStream, cv.MimeType ?? "application/octet-stream", cv.FileName!);
        }

        // --- EDITOR LOGIC ---
        public async Task<CvEditorDataDto?> GetEditorDataAsync(Guid cvId)
        {
            var cv = await _cvRepository.GetCvByIdAsync(cvId);
            if (cv == null) return null;

            return await BuildEditorData(cv);
        }

        public async Task<CvEditorDataDto?> UpdateEditorDataAsync(Guid cvId, UpdateCvEditorRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("Họ tên không được để trống.");

            var cv = await _cvRepository.GetCvByIdAsync(cvId);
            if (cv == null) return null;

            cv.FullName = request.FullName;
            cv.Position = request.Position;
            cv.EducationSummary = request.Summary;
            cv.UpdatedAt = DateTime.UtcNow;

            var educations = request.Educations.Select(x => new CvEducation
            {
                Id = x.Id ?? Guid.NewGuid(),
                Cvid = cvId,
                SchoolName = x.SchoolName,
                Major = x.Major,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Description = x.Description
            });

            var experiences = request.Experiences.Select(x => new CvExperience
            {
                Id = x.Id ?? Guid.NewGuid(),
                Cvid = cvId,
                CompanyName = x.CompanyName,
                Position = x.Position,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Description = x.Description
            });

            var projects = request.Projects.Select(x => new CvProject
            {
                Id = x.Id ?? Guid.NewGuid(),
                Cvid = cvId,
                ProjectName = x.ProjectName,
                Role = x.Role,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Description = x.Description
            });

            var certificates = request.Certificates.Select(x => new CvCertificate
            {
                Id = x.Id ?? Guid.NewGuid(),
                Cvid = cvId,
                CertificateName = x.CertificateName,
                Organization = x.Organization,
                IssueDate = x.IssueDate,
                ExpiredDate = x.ExpiredDate
            });

            var skills = request.Skills
                .Where(x => !string.IsNullOrWhiteSpace(x.SkillName))
                .Select(x => new CvSkill
                {
                    Cvid = cvId,
                    SkillName = x.SkillName.Trim(),
                    Level = x.Level
                });

            await _cvRepository.UpdateEditorSectionsAsync(cv, educations, experiences, projects, certificates, skills);

            return await BuildEditorData(cv);
        }

        // --- PRIVATE MAPPING METHODS ---
        private async Task<CvDetailDto> MapToDetailDto(Cv cv)
        {
            var fileBucket = _configuration["Minio:CvBucket"] ?? "cvs";
            var displayFileUrl = string.IsNullOrWhiteSpace(cv.FileUrl)
                ? null
                : await _minioService.GetUrlImage(fileBucket, cv.FileUrl);

            return new CvDetailDto
            {
                Id = cv.Id,
                CandidateId = cv.CandidateId,
                FullName = cv.FullName,
                Email = cv.Email,
                PhoneNumber = cv.PhoneNumber,
                Address = cv.Address,
                Birthday = cv.Birthday,
                Gender = cv.Gender,
                Nationality = cv.Nationality,
                Position = cv.Position,
                ExperienceYears = cv.ExperienceYears,
                EducationSummary = cv.EducationSummary,
                Field = cv.Field,
                CurrentSalary = cv.CurrentSalary,
                FileName = cv.FileName,
                FileUrl = displayFileUrl,
                MimeType = cv.MimeType,
                IsDefault = cv.IsDefault ?? false,
                CreatedAt = cv.CreatedAt,
                UpdatedAt = cv.UpdatedAt
            };
        }

        private async Task<CvEditorDataDto> BuildEditorData(Cv cv)
        {
            var rawEducations = await _cvRepository.GetEducationsAsync(cv.Id);
            var rawExperiences = await _cvRepository.GetExperiencesAsync(cv.Id);
            var rawProjects = await _cvRepository.GetProjectsAsync(cv.Id);
            var rawCertificates = await _cvRepository.GetCertificatesAsync(cv.Id);
            var rawSkills = await _cvRepository.GetSkillsAsync(cv.Id);

            return new CvEditorDataDto
            {
                CvId = cv.Id,
                CandidateId = cv.CandidateId,
                FullName = cv.FullName,
                Position = cv.Position,
                Summary = cv.EducationSummary,
                Educations = rawEducations.Select(x => new CvEducationItemDto
                {
                    Id = x.Id,
                    SchoolName = x.SchoolName,
                    Major = x.Major,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Description = x.Description
                }).ToList(),
                Experiences = rawExperiences.Select(x => new CvExperienceItemDto
                {
                    Id = x.Id,
                    CompanyName = x.CompanyName,
                    Position = x.Position,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Description = x.Description
                }).ToList(),
                Projects = rawProjects.Select(x => new CvProjectItemDto
                {
                    Id = x.Id,
                    ProjectName = x.ProjectName,
                    Role = x.Role,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Description = x.Description
                }).ToList(),
                Certificates = rawCertificates.Select(x => new CvCertificateItemDto
                {
                    Id = x.Id,
                    CertificateName = x.CertificateName,
                    Organization = x.Organization,
                    IssueDate = x.IssueDate,
                    ExpiredDate = x.ExpiredDate
                }).ToList(),
                Skills = rawSkills.Select(x => new CvSkillItemDto
                {
                    SkillName = x.SkillName,
                    Level = x.Level
                }).ToList()
            };
        }
    }
}
