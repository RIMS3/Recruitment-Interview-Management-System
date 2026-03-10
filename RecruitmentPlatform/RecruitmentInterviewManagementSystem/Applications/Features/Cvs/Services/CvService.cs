using Microsoft.EntityFrameworkCore;
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
        private readonly FakeTopcvContext _context;

        public CvService(ICvRepository cvRepository, IMinIOCV minioService, IConfiguration configuration, FakeTopcvContext context)
        {
            _cvRepository = cvRepository;
            _minioService = minioService;
            _configuration = configuration;
            _context = context;
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
                UpdatedAt = cv.UpdatedAt,
                TemplateId = cv.TemplateId // 👉 1. THÊM DÒNG NÀY: Trả TemplateId về cho trang Manage-CV
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
                TemplateId = request.TemplateId ?? "tpl-1", // 👉 2. THÊM DÒNG NÀY: Hứng mã Mẫu khi Tạo mới CV
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

            // 👉 3. THÊM ĐOẠN NÀY: Cập nhật TemplateId nếu có thay đổi
            if (!string.IsNullOrWhiteSpace(request.TemplateId))
            {
                cv.TemplateId = request.TemplateId;
            }

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

            // 1. MAPPING ĐẦY ĐỦ CÁC TRƯỜNG THÔNG TIN CÁ NHÂN VÀO ENTITY
            cv.FullName = request.FullName;
            cv.Position = request.Position;
            cv.EducationSummary = request.Summary;
            cv.Email = request.Email;
            cv.PhoneNumber = request.PhoneNumber;
            cv.Address = request.Address;
            cv.Birthday = request.Birthday;
            cv.Gender = request.Gender;
            cv.Nationality = request.Nationality;
            cv.Field = request.Field;
            cv.CurrentSalary = request.CurrentSalary;
            cv.ExperienceYears = request.ExperienceYears;

            // 👉 4. THÊM ĐOẠN NÀY: Hứng TemplateId khi nhấn nút "Lưu CV"
            if (!string.IsNullOrWhiteSpace(request.TemplateId))
            {
                cv.TemplateId = request.TemplateId;
            }

            cv.UpdatedAt = DateTime.UtcNow;

            // 2. MAPPING MẢNG (GIỮ NGUYÊN)
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

            // Gọi Repository xử lý Update db
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
                UpdatedAt = cv.UpdatedAt,
                TemplateId = cv.TemplateId // 👉 5. THÊM DÒNG NÀY
            };
        }

        private async Task<CvEditorDataDto> BuildEditorData(Cv cv)
        {
            var rawEducations = await _cvRepository.GetEducationsAsync(cv.Id);
            var rawExperiences = await _cvRepository.GetExperiencesAsync(cv.Id);
            var rawProjects = await _cvRepository.GetProjectsAsync(cv.Id);
            var rawCertificates = await _cvRepository.GetCertificatesAsync(cv.Id);
            var rawSkills = await _cvRepository.GetSkillsAsync(cv.Id);

            // 👉 1. BỔ SUNG XỬ LÝ LẤY LINK ẢNH TỪ MINIO
            var fileBucket = _configuration["Minio:CvBucket"] ?? "cvs";
            var displayFileUrl = string.IsNullOrWhiteSpace(cv.FileUrl)
                ? null
                : await _minioService.GetUrlImage(fileBucket, cv.FileUrl);

            return new CvEditorDataDto
            {
                CvId = cv.Id,
                CandidateId = cv.CandidateId,
                FullName = cv.FullName,
                Position = cv.Position,
                Summary = cv.EducationSummary,

                // BỔ SUNG TRẢ VỀ CHO FRONTEND ĐỂ F5 KHÔNG BỊ MẤT DỮ LIỆU
                Email = cv.Email,
                PhoneNumber = cv.PhoneNumber,
                Address = cv.Address,
                Birthday = cv.Birthday,
                Gender = cv.Gender,
                Nationality = cv.Nationality,
                Field = cv.Field,
                CurrentSalary = cv.CurrentSalary,
                ExperienceYears = cv.ExperienceYears,

                TemplateId = cv.TemplateId,

                // 👉 2. THÊM 2 DÒNG NÀY ĐỂ TRẢ VỀ CHO REACT TẠO ẢNH PREVIEW
                FileName = cv.FileName,
                FileUrl = displayFileUrl,

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
        public async Task<CvAvatarResponseDto> UpdateAvatarAsync(Guid cvId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Vui lòng chọn một file ảnh hợp lệ.");

            // 1. Tìm CV (Đổi .Cvs thành DbSet tương ứng trong context của bạn nếu khác)
            var cv = await _context.Cvs.FindAsync(cvId);
            if (cv == null)
                throw new KeyNotFoundException("Không tìm thấy CV.");

            string bucketName = "cvs"; // Có thể đổi thành "avatars" nếu muốn

            // 2. Xóa ảnh cũ trên MinIO (nếu có)
            if (!string.IsNullOrWhiteSpace(cv.FileName))
            {
                await _minioService.DeleteAsync(cv.FileName, bucketName);
            }

            // 3. Upload ảnh mới
            string newObjectName = await _minioService.UploadAsync(file, bucketName);

            // 4. Cập nhật Database
            cv.FileName = file.FileName; // Lưu tên gốc (VD: anh-the.jpg)
            cv.FileUrl = newObjectName;  // Lưu mã GUID của MinIO vào cột FileUrl
            cv.MimeType = file.ContentType;
            cv.UpdatedAt = DateTime.UtcNow;

            _context.Cvs.Update(cv);
            await _context.SaveChangesAsync();

            // 5. Lấy URL Presigned từ MinIO
            string fileUrl = await _minioService.GetUrlImage(bucketName, newObjectName);

            // 6. Trả về DTO
            return new CvAvatarResponseDto
            {
                Id = cv.Id,
                CandidateId = cv.CandidateId,
                FileName = cv.FileName,
                FileUrl = fileUrl,
                MimeType = cv.MimeType,
                UpdatedAt = cv.UpdatedAt
            };
        }
    }
}