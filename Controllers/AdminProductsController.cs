using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopCoAPI.Data;
using ShopCoAPI.DTOs;
using ShopCoAPI.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopCoAPI.Service;

namespace ShopCoAPI.Controllers
{
    [Route("api/admin/products")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : ControllerBase
    {
        private readonly ShopCoDBContext _context;
        private readonly IR2StorageService _r2;
        private readonly ILogger<AdminProductsController> _logger;

        public AdminProductsController(ShopCoDBContext context, IR2StorageService r2, ILogger<AdminProductsController> logger)
        {
            _context = context;
            _r2 = r2;
            _logger = logger;
        }

        [HttpPost]
        [RequestSizeLimit(20_000_000)] // 20 MB
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductResponseDto>> Create([FromForm] ProductCreateUpdateDto dto, [FromForm] IFormFile? image)
        {
            if (!Request.HasFormContentType)
            {
                var actual = Request.ContentType ?? "(none)";
                _logger.LogWarning("Create product: unsupported content type {ContentType}", actual);
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, $"Expected multipart/form-data content type. Actual: {actual}");
            }

            if (image != null)
            {
                var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!allowed.Contains(image.ContentType))
                {
                    _logger.LogWarning("Create product: unsupported image content type {ImageContentType}", image.ContentType);
                    return StatusCode(StatusCodes.Status415UnsupportedMediaType, $"Unsupported image content type. Allowed: image/jpeg, image/png, image/webp. Actual: {image.ContentType}");
                }
            }
            if (dto is null)
                return BadRequest();

            string imageUrl = dto.ImageUrl;
            if (image != null)
            {
                imageUrl = await _r2.UploadFileAsync(image);
            }

            var product = new Products
            {
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                Price = dto.Price,
                Quantity = dto.Quantity,
                ImageUrl = imageUrl,
                Rating = dto.Rating
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var response = new ProductResponseDto
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Category = product.Category,
                Price = product.Price,
                Quantity = product.Quantity,
                ImageUrl = product.ImageUrl,
                Rating = product.Rating
            };

            return CreatedAtAction("GetProductById", "Products", new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [RequestSizeLimit(20_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductCreateUpdateDto dto, [FromForm] IFormFile? image)
        {
            if (!Request.HasFormContentType)
            {
                var actual = Request.ContentType ?? "(none)";
                _logger.LogWarning("Update product {Id}: unsupported content type {ContentType}", id, actual);
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, $"Expected multipart/form-data content type. Actual: {actual}");
            }

            if (image != null)
            {
                var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!allowed.Contains(image.ContentType))
                {
                    _logger.LogWarning("Update product {Id}: unsupported image content type {ImageContentType}", id, image.ContentType);
                    return StatusCode(StatusCodes.Status415UnsupportedMediaType, $"Unsupported image content type. Allowed: image/jpeg, image/png, image/webp. Actual: {image.ContentType}");
                }
            }
            var product = await _context.Products.FindAsync(id);
            if (product is null)
                return NotFound();

            product.Title = dto.Title;
            product.Description = dto.Description;
            product.Category = dto.Category;
            product.Price = dto.Price;
            product.Quantity = dto.Quantity;

            if (image != null)
            {
                product.ImageUrl = await _r2.UploadFileAsync(image);
            }

            product.Rating = dto.Rating;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
