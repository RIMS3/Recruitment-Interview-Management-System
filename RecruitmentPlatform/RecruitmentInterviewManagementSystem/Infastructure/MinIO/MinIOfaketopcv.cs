using Minio;
using Minio.DataModel.Args;
using RecruitmentInterviewManagementSystem.Applications.Features.Interface;

namespace RecruitmentInterviewManagementSystem.Infastructure.MinIO
{
    public class MinIOfaketopcv : IMinIOCV
    {
        private readonly IMinioClient _clientMinIO;

        // Chỉ cần tiêm IMinioClient là đủ
        public MinIOfaketopcv(IMinioClient minioClient)
        {
            _clientMinIO = minioClient;
        }

        public async Task<string> UploadAsync(IFormFile file, string bucketName)
        {
            var objectName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            using var stream = file.OpenReadStream();

            // Tự động kiểm tra và tạo bucket (cvs, avatars, documents...) nếu chưa có
            var found = await _clientMinIO.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (!found)
            {
                await _clientMinIO.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
            }

            await _clientMinIO.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(file.ContentType)
            );

            return objectName;
        }

        public async Task<string?> GetUrlImage(string bucketName, string objectName)
        {
            // Trả về null thay vì ảnh Pinterest. Frontend sẽ tự check để hiển thị UI phù hợp.
            if (string.IsNullOrWhiteSpace(bucketName) || string.IsNullOrWhiteSpace(objectName))
                return null;

            try
            {
                return await _clientMinIO.PresignedGetObjectAsync(
                    new PresignedGetObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithExpiry(60 * 60) // Link sống trong 1 giờ
                );
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task DeleteAsync(string objectName, string bucketName)
        {
            if (string.IsNullOrWhiteSpace(objectName)) return;

            try
            {
                await _clientMinIO.RemoveObjectAsync(
                    new RemoveObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName));
            }
            catch (Exception)
            {
                // Bỏ qua nếu file không tồn tại
            }
        }

        public async Task GetObjectStreamAsync(string bucketName, string objectName, Action<Stream> callback)
        {
            await _clientMinIO.GetObjectAsync(new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(callback));
        }
    }
}