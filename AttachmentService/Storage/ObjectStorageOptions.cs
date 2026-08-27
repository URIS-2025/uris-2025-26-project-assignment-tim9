namespace AttachmentService.Storage
{
 
    public class ObjectStorageOptions
    {

        // Endpoint the server uses to reach object storage directly (HeadObject,
        // server-to-server calls). In docker-compose this is the internal
        // container hostname, e.g. "http://minio:9000".
        public string ServiceUrl { get; set; } = string.Empty;

        // Endpoint baked into the Host header of presigned upload/download URLs
        // that get handed to the browser. Must be reachable from wherever the
        // browser runs (e.g. "http://localhost:9000" for local docker-compose
        // dev), which is not necessarily the same as ServiceUrl. Falls back to
        // ServiceUrl when unset, which is correct whenever both client and
        // server can already reach storage through the same address (e.g. a
        // real S3 endpoint).
        public string PublicServiceUrl { get; set; } = string.Empty;

        public string EffectivePublicServiceUrl =>
            string.IsNullOrWhiteSpace(PublicServiceUrl) ? ServiceUrl : PublicServiceUrl;

        public string BucketName { get; set; } = string.Empty;

        public string AccessKey { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;

        public bool ForcePathStyle { get; set; } = true;
    }
}
