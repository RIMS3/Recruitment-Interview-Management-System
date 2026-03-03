using Minio;
using Minio.DataModel.Args;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;

namespace RecruitmentInterviewManagementSystem.Infastructure.MinIO
{
    public class MinIOfaketopcv : IMinIOCV
    {
        private readonly IConfiguration _configuration;
        private readonly IMinioClient _clientMinIO;

        public MinIOfaketopcv(IConfiguration configuration, IMinioClient minioClient)
        {
            _configuration = configuration;
            _clientMinIO = minioClient;
        }

        public async Task<string> UploadAsync(IFormFile file )
        {
            // Lấy tên bucket từ appsettings, nếu không có thì mặc định là "avatars"
            var bucketName = _configuration["Minio:Bucket"] ?? "avatars";
            var objectName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            using var stream = file.OpenReadStream();

            // Kiểm tra bucket tồn tại chưa, chưa thì tự tạo
            var found = await _clientMinIO.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (!found)
            {
                await _clientMinIO.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
            }

            // Đẩy file lên Docker MinIO
            await _clientMinIO.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(file.ContentType)
            );

            // Trả về tên file (VD: a1b2c3d4.jpg)
            return objectName;
        }

        public async Task<string> GetUrlImage(string bucket, string imageName)
        {
            string defaultImageUrl = "https://i.pinimg.com/236x/8b/cf/15/8bcf15e8af97cbd56ab29f15e01933aa.jpg";

            if (string.IsNullOrWhiteSpace(bucket) || string.IsNullOrWhiteSpace(imageName))
                return defaultImageUrl;

            try
            {
                // Sinh link ảnh tồn tại trong 1 giờ (bảo mật cao)
                var url = await _clientMinIO.PresignedGetObjectAsync(
                    new PresignedGetObjectArgs()
                        .WithBucket(bucket)
                        .WithObject(imageName)
                        .WithExpiry(60 * 60)
                );
                return url;
            }
            catch (Exception)
            {
                // Nếu ảnh bị lỗi hoặc không tìm thấy, trả về ảnh mặc định
                return defaultImageUrl;
            }
        }

        public async Task DeleteAsync(string objectName)
        {
            var bucketName = _configuration["Minio:Bucket"] ?? "avatars";
            try
            {
                await _clientMinIO.RemoveObjectAsync(
                    new RemoveObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName));
            }
            catch (Exception)
            {
                // Bỏ qua nếu lỗi xóa (ví dụ file đã bị xóa từ trước)
            }
        }
    }
}