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

        public async Task<string?> GetUrlImage(string bucket, string imageName)
        {
            if (string.IsNullOrWhiteSpace(bucket) || string.IsNullOrWhiteSpace(imageName))
                return "https://i.pinimg.com/236x/8b/cf/15/8bcf15e8af97cbd56ab29f15e01933aa.jpg";

            try
            {

                await _clientMinIO.StatObjectAsync(
                new StatObjectArgs()
                        .WithBucket(bucket)
                        .WithObject(imageName)
                );

                var url = await _clientMinIO.PresignedGetObjectAsync(
                    new PresignedGetObjectArgs()
                        .WithBucket(bucket)
                        .WithObject(imageName)
                        .WithExpiry(60 * 60)
                );

                return url;
            }
            catch (Minio.Exceptions.ObjectNotFoundException)
            {
                return "https://i.pinimg.com/236x/8b/cf/15/8bcf15e8af97cbd56ab29f15e01933aa.jpg";
            }
            catch (Exception ex)
            {

                return "https://i.pinimg.com/236x/8b/cf/15/8bcf15e8af97cbd56ab29f15e01933aa.jpg";
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