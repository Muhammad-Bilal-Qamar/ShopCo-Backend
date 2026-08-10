namespace ShopCoAPI.Settings
{
    public class CloudflareR2Settings
    {
        public string AccountId { get; set; } = string.Empty;
        public string AccessKeyId { get; set; } = string.Empty;
        public string SecretAccessKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;

        // S3-compatible API endpoint used for uploads (PutObject etc), NOT for serving images.
        // e.g. https://<accountId>.r2.cloudflarestorage.com
        public string ServiceUrl { get; set; } = string.Empty;

        // Public-read base URL used to SERVE images to the browser. Two supported options:
        //  1. R2.dev dev subdomain (enable "Public Access" on the bucket in the R2 dashboard):
        //     https://pub-xxxxxxxxxxxxxxxx.r2.dev
        //  2. A custom domain mapped to the bucket (recommended for production):
        //     https://cdn.yourdomain.com
        // No trailing slash.
        public string PublicDomainUrl { get; set; } = string.Empty;
    }
}