using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using AttachmentService.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AttachmentService.Storage
{
    public class S3FileStorageService : IFileStorageService
    {
        // Used for server-to-server calls (HeadObject). In docker-compose this
        // reaches storage via the internal container hostname.
        private readonly IAmazonS3 _internalClient;

        // Used only to sign upload/download URLs handed to the browser. Its
        // endpoint is baked into the Host header of the signature, so it must
        // be an address the browser can resolve - see ObjectStorageOptions.PublicServiceUrl.
        private readonly IAmazonS3 _presigningClient;
        private readonly ObjectStorageOptions _options;

        public S3FileStorageService(
            [FromKeyedServices(S3ClientKeys.Internal)] IAmazonS3 internalClient,
            [FromKeyedServices(S3ClientKeys.Public)] IAmazonS3 presigningClient,
            IOptions<ObjectStorageOptions> options)
        {
            _internalClient = internalClient;
            _presigningClient = presigningClient;
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
                Protocol = ResolveProtocol(_options.EffectivePublicServiceUrl)
            };

            return _presigningClient.GetPreSignedURL(request);
        }

        public string GenerateDownloadUrl(string storagePath, TimeSpan? expiry = null)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = storagePath,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromMinutes(15)),
                Protocol = ResolveProtocol(_options.EffectivePublicServiceUrl)
            };

            return _presigningClient.GetPreSignedURL(request);
        }

        public async Task<bool> ObjectExistsAsync(string storagePath)
        {
            try
            {
                await _internalClient.GetObjectMetadataAsync(new GetObjectMetadataRequest
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

        private static Protocol ResolveProtocol(string serviceUrl)
        {
            return serviceUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? Protocol.HTTPS
                : Protocol.HTTP;
        }
    }
}
