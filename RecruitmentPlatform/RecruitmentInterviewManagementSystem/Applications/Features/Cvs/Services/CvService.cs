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
            // 1. Kiểm tra xem cái candidateId truyền vào là ProfileId hay UserId
            // Đầu tiên, thử tìm Profile theo UserId
            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == candidateId);

            Guid actualProfileId;

            if (profile != null)
            {
                // Nếu tìm thấy profile theo UserId, thì dùng Id của Profile đó
                actualProfileId = profile.Id;
            }
            else
            {
                // Nếu không thấy, có thể ID truyền vào đã là ProfileId rồi
                actualProfileId = candidateId;
            }

            // 2. Truy vấn danh sách CV bằng actualProfileId (ID chuẩn của bảng Profile)
            var rawCvs = await _cvRepository.GetCvsByCandidateIdAsync(actualProfileId);

            // 3. Lọc bỏ các CV đã xóa mềm (lọc các CV có SĐT là cờ bản nháp)
            //var activeCvs = rawCvs.Where(cv => cv.IsDeleted == false);
            var activeCvs = rawCvs.Where(cv => cv.IsDeleted == false && cv.PhoneNumber != "[BẢN NHÁP]");

            return activeCvs.Select(cv => new CvSummaryDto
            {
                Id = cv.Id,
                CandidateId = cv.CandidateId,
                FullName = cv.FullName,
                Position = cv.Position,
                ExperienceYears = cv.ExperienceYears,
                IsDefault = cv.IsDefault ?? false,
                UpdatedAt = cv.UpdatedAt,
                TemplateId = cv.TemplateId
            });
        }

        public async Task<CvDetailDto?> GetCvByIdAsync(Guid id)
        {
            var cv = await _cvRepository.GetCvByIdAsync(id);

            // 👉 KIỂM TRA: Nếu không có hoặc đã bị xóa mềm thì trả về null
            if (cv == null || cv.IsDeleted) return null;

            return await MapToDetailDto(cv);
        }

        public async Task<CvDetailDto> CreateCvAsync(CreateCvRequest request)
        {
            // 1. Kiểm tra đầu vào cơ bản
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("Họ tên không được để trống.");

            // 2. XỬ LÝ MAPPING ID: Tìm Profile ứng viên dựa trên ID gửi từ FE (đang là UserId)
            // Chúng ta tìm trong bảng CandidateProfiles xem có dòng nào có UserId trùng với request.CandidateId không
            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == request.CandidateId);

            // Nếu không tìm thấy theo UserId, thử kiểm tra xem bản thân request.CandidateId có phải là ProfileId không
            if (profile == null)
            {
                profile = await _context.CandidateProfiles
                    .FirstOrDefaultAsync(p => p.Id == request.CandidateId);
            }

            // Nếu vẫn không thấy thì mới báo lỗi thực sự
            if (profile == null)
                throw new KeyNotFoundException("Không tìm thấy hồ sơ ứng viên (Profile) tương ứng với tài khoản này.");

            // Lưu lại ID thực sự của bảng Profile
            var actualProfileId = profile.Id;

            // 3. Khởi tạo thực thể CV
            var cv = new Cv
            {
                Id = Guid.NewGuid(),
                CandidateId = actualProfileId, // Dùng ID của Profile để lưu vào DB
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
                TemplateId = request.TemplateId ?? "tpl-1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            // 4. Xử lý File đính kèm (nếu có) qua MinIO
            if (request.File != null && request.File.Length > 0)
            {
                var bucketName = _configuration["Minio:CvBucket"] ?? "cvs";
                var objectName = await _minioService.UploadAsync(request.File, bucketName);

                cv.FileName = request.File.FileName;
                cv.MimeType = request.File.ContentType;
                cv.FileUrl = objectName;
            }

            // 5. Nếu CV này là mặc định, reset các CV khác của ứng viên này
            if (cv.IsDefault == true)
            {
                await _cvRepository.ResetDefaultCvForCandidateAsync(actualProfileId);
            }

            // 6. Lưu vào cơ sở dữ liệu
            await _cvRepository.AddCvAsync(cv);

            // 7. Map kết quả trả về DTO cho Frontend
            return await MapToDetailDto(cv);
        }

        public async Task<CvDetailDto?> UpdateCvAsync(Guid id, UpdateCvRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("Họ tên không được để trống.");

            var cv = await _cvRepository.GetCvByIdAsync(id);

            // Không cho phép update nếu CV đã bị xóa mềm
            if (cv == null || cv.IsDeleted) return null;

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

            // 👉 NẾU KHÔNG TÌM THẤY HOẶC ĐÃ XÓA TRƯỚC ĐÓ THÌ BỎ QUA
            if (cv == null || cv.IsDeleted) return false;

            // 👉 XÓA MỀM: Chuyển trạng thái IsDeleted thành true thay vì xóa vật lý
            cv.IsDeleted = true;
            cv.UpdatedAt = DateTime.UtcNow;

            // Giữ nguyên file trên MinIO để các đơn ứng tuyển cũ vẫn truy cập được ảnh/file

            await _cvRepository.UpdateCvAsync(cv);
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

            // 👉 CHẶN TRUY CẬP EDITOR NẾU CV ĐÃ BỊ XÓA
            if (cv == null || cv.IsDeleted) return null;

            return await BuildEditorData(cv);
        }

        public async Task<CvEditorDataDto?> UpdateEditorDataAsync(Guid cvId, UpdateCvEditorRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("Họ tên không được để trống.");

            var cv = await _cvRepository.GetCvByIdAsync(cvId);

            // Không cho phép update editor nếu CV đã bị xóa mềm
            if (cv == null || cv.IsDeleted) return null;

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

            if (!string.IsNullOrWhiteSpace(request.TemplateId))
            {
                cv.TemplateId = request.TemplateId;
            }

            cv.UpdatedAt = DateTime.UtcNow;

            // 2. MAPPING MẢNG
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
                TemplateId = cv.TemplateId
            };
        }

        private async Task<CvEditorDataDto> BuildEditorData(Cv cv)
        {
            var rawEducations = await _cvRepository.GetEducationsAsync(cv.Id);
            var rawExperiences = await _cvRepository.GetExperiencesAsync(cv.Id);
            var rawProjects = await _cvRepository.GetProjectsAsync(cv.Id);
            var rawCertificates = await _cvRepository.GetCertificatesAsync(cv.Id);
            var rawSkills = await _cvRepository.GetSkillsAsync(cv.Id);

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

            var cv = await _context.Cvs.FindAsync(cvId);

            // 👉 KIỂM TRA ĐẢM BẢO CV CHƯA BỊ XÓA
            if (cv == null || cv.IsDeleted)
                throw new KeyNotFoundException("Không tìm thấy CV.");

            string bucketName = "cvs";

            if (!string.IsNullOrWhiteSpace(cv.FileName))
            {
                await _minioService.DeleteAsync(cv.FileName, bucketName);
            }

            string newObjectName = await _minioService.UploadAsync(file, bucketName);

            cv.FileName = file.FileName;
            cv.FileUrl = newObjectName;
            cv.MimeType = file.ContentType;
            cv.UpdatedAt = DateTime.UtcNow;

            _context.Cvs.Update(cv);
            await _context.SaveChangesAsync();

            string fileUrl = await _minioService.GetUrlImage(bucketName, newObjectName);

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