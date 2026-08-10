using Microsoft.AspNetCore.Http;

namespace ShopCoAPI.Service
{
    public interface IR2StorageService
    {
        /// <summary>
        /// Uploads the provided file to Cloudflare R2 and returns a publicly accessible URL.
        /// </summary>
        Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an object from R2. Accepts either a raw storage key or a full public URL.
        /// </summary>
        Task<bool> DeleteFileAsync(string keyOrUrl, CancellationToken cancellationToken = default);
    }
}