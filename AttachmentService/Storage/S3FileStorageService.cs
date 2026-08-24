using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using AttachmentService.Exceptions;
using Microsoft.Extensions.Options;

namespace AttachmentService.Storage
{
    public class S3FileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ObjectStorageOptions _options;

        public S3FileStorageService(IAmazonS3 s3Client, IOptions<ObjectStorageOptions> options)
        {
            _s3Client = s3Client;
            _options = options.Value;
        }

        public string GenerateUploadUrl(string storagePath, string contentType, TimeSpan? expiry = null)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = storagePath,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromMinutes(15)),
                ContentType = contentType,
                Protocol = ResolveProtocol()
            };

            return _s3Client.GetPreSignedURL(request);
        }

        public string GenerateDownloadUrl(string storagePath, TimeSpan? expiry = null)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = storagePath,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromMinutes(15)),
                Protocol = ResolveProtocol()
            };

            return _s3Client.GetPreSignedURL(request);
        }

        public async Task<bool> ObjectExistsAsync(string storagePath)
        {
            try
            {
                await _s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = _options.BucketName,
                    Key = storagePath
                });

                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex) when (ex is AmazonServiceException or HttpRequestException or TaskCanceledException)
            {
                throw new StorageUnavailableException(ex);
            }
        }

        private Protocol ResolveProtocol()
        {
            return _options.ServiceUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? Protocol.HTTPS
                : Protocol.HTTP;
        }
    }
}
