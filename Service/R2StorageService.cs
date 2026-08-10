using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ShopCoAPI.Settings;

namespace ShopCoAPI.Service
{
    public class R2StorageService : IR2StorageService
    {
        private readonly CloudflareR2Settings _settings;

        public R2StorageService(IOptions<CloudflareR2Settings> options)
        {
            _settings = options.Value;
        }

        private AmazonS3Client CreateClient()
        {
            var creds = new BasicAWSCredentials(_settings.AccessKeyId, _settings.SecretAccessKey);
            var config = new AmazonS3Config
            {
                ServiceURL = _settings.ServiceUrl, // API endpoint, e.g. https://<accountId>.r2.cloudflarestorage.com
                ForcePathStyle = true,
                // R2 doesn't support the SDK's default streaming/chunked checksum-trailer uploads
                // (STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER). Forcing "WHEN_REQUIRED" makes the
                // SDK fall back to a normal, non-chunked signed request that R2 understands.
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED
            };
            return new AmazonS3Client(creds, config);
        }

        public async Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is null or empty", nameof(file));

            var extension = Path.GetExtension(file.FileName);
            var key = $"{Guid.NewGuid():N}{extension}";

            using var client = CreateClient();
            using var stream = file.OpenReadStream();

            var putRequest = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType,
                DisablePayloadSigning = true // avoids chunked/streaming signing R2 rejects
            };

            var response = await client.PutObjectAsync(putRequest, cancellationToken);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK && response.HttpStatusCode != System.Net.HttpStatusCode.Created)
                throw new InvalidOperationException($"Failed to upload file to R2. Status: {response.HttpStatusCode}");

            // Return a PERMANENT public URL — never a presigned URL. Presigned URLs expire
            // (max 7 days on AWS, and R2 enforces its own cap too), so persisting one to the
            // database guarantees the image will eventually break with an ExpiredRequest XML
            // response, which browsers surface as net::ERR_BLOCKED_BY_ORB on an <img> tag.
            return BuildPublicUrl(key);
        }

        public async Task<bool> DeleteFileAsync(string keyOrUrl, CancellationToken cancellationToken = default)
        {
            var key = ExtractKey(keyOrUrl);
            using var client = CreateClient();
            var response = await client.DeleteObjectAsync(_settings.BucketName, key, cancellationToken);
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent
                || response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        private string BuildPublicUrl(string key)
        {
            return $"{_settings.PublicDomainUrl.TrimEnd('/')}/{key}";
        }

        // Accepts either a raw key or a full URL (handles data saved before this migration)
        // so DeleteFileAsync keeps working against old rows.
        private string ExtractKey(string keyOrUrl)
        {
            if (Uri.TryCreate(keyOrUrl, UriKind.Absolute, out var uri))
                return uri.Segments[^1].TrimStart('/');
            return keyOrUrl;
        }
    }
}