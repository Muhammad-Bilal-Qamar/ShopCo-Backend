//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using ShopCoAPI.Data;
//using ShopCoAPI.DTOs;
//using ShopCoAPI.Models;

//namespace ShopCoAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ProductsController : ControllerBase
//    {
//        private readonly ShopCoDBContext _Context;

//        public ProductsController(ShopCoDBContext context)
//        {
//            _Context = context;
//        }

//        [HttpGet]
//        [AllowAnonymous]
//        public async Task<ActionResult<List<ProductResponseDto>>> GetProducts()
//        {
//            var products = await _Context.Products.ToListAsync();

//            var response = products.Select(p => new ProductResponseDto
//            {
//                Id = p.Id,
//                Title = p.Title,
//                Description = p.Description,
//                Category = p.Category,
//                Price = p.Price,
//                Quantity = p.Quantity,
//                ImageUrl = p.ImageUrl,
//                Rating = p.Rating
//            }).ToList();

//            return Ok(response);
//        }

//        [HttpGet("{id}")]
//        [AllowAnonymous]
//        public async Task<ActionResult<ProductResponseDto>> GetProductById(int id)
//        {
//            var product = await _Context.Products.FindAsync(id);
//            if (product is null)
//                return NotFound();

//            var response = new ProductResponseDto
//            {
//                Id = product.Id,
//                Title = product.Title,
//                Description = product.Description,
//                Category = product.Category,
//                Price = product.Price,
//                Quantity = product.Quantity,
//                ImageUrl = product.ImageUrl,
//                Rating = product.Rating
//            };

//            return Ok(response);
//        }

//        [HttpPost]
//        [Authorize(Policy = "AdminOnly")]
//        public async Task<ActionResult<ProductResponseDto>> AddProduct(ProductCreateUpdateDto inputDto)
//        {
//            if (inputDto is null)
//                return BadRequest();

//            var newProduct = new Products
//            {
//                Title = inputDto.Title,
//                Description = inputDto.Description,
//                Category = inputDto.Category,
//                Price = inputDto.Price,
//                Quantity = inputDto.Quantity,
//                ImageUrl = inputDto.ImageUrl,
//                Rating = new ProductRating
//                {
//                    Rate = inputDto.Rating.Rate,
//                    Count = inputDto.Rating.Count
//                }
//            };

//            _Context.Products.Add(newProduct);
//            await _Context.SaveChangesAsync();

//            var response = new ProductResponseDto
//            {
//                Id = newProduct.Id,
//                Title = newProduct.Title,
//                Description = newProduct.Description,
//                Category = newProduct.Category,
//                Price = newProduct.Price,
//                Quantity = newProduct.Quantity,
//                ImageUrl = newProduct.ImageUrl,
//                Rating = newProduct.Rating
//            };

//            return CreatedAtAction(nameof(GetProductById), new { id = response.Id }, response);
//        }

//        [HttpPut("{id}")]
//        [Authorize(Policy = "AdminOnly")]
//        public async Task<IActionResult> UpdateProduct(int id, ProductCreateUpdateDto updatedDto)
//        {
//            if (updatedDto is null)
//                return BadRequest();

//            var product = await _Context.Products.FindAsync(id);
//            if (product is null)
//                return NotFound();

//            product.Title = updatedDto.Title;
//            product.Description = updatedDto.Description;
//            product.Category = updatedDto.Category;
//            product.Price = updatedDto.Price;
//            product.Quantity = updatedDto.Quantity;
//            product.ImageUrl = updatedDto.ImageUrl;
//            product.Rating.Rate = updatedDto.Rating.Rate;
//            product.Rating.Count = updatedDto.Rating.Count;

//            await _Context.SaveChangesAsync();

//            return NoContent();
//        }

//        [HttpDelete("{id}")]
//        [Authorize(Policy = "AdminOnly")]
//        public async Task<IActionResult> DeleteProduct(int id)
//        {
//            var product = await _Context.Products.FindAsync(id);
//            if (product is null)
//                return NotFound();

//            _Context.Products.Remove(product);
//            await _Context.SaveChangesAsync();

//            return NoContent();
//        }
//    }
//}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopCoAPI.Data;
using ShopCoAPI.DTOs;
using ShopCoAPI.Models;

namespace ShopCoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopCoDBContext _Context;

        public ProductsController(ShopCoDBContext context)
        {
            _Context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductResponseDto>>> GetProducts()
        {
            var products = await _Context.Products.ToListAsync();

            var response = products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Category = p.Category,
                Price = p.Price,
                Quantity = p.Quantity,
                ImageUrl = p.ImageUrl,
                Rating = p.Rating
            }).ToList();

            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductResponseDto>> GetProductById(int id)
        {
            var product = await _Context.Products.FindAsync(id);
            if (product is null)
                return NotFound();

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

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ProductResponseDto>> AddProduct(ProductCreateUpdateDto inputDto)
        {
            if (inputDto is null)
                return BadRequest();

            var newProduct = new Products
            {
                Title = inputDto.Title,
                Description = inputDto.Description,
                Category = inputDto.Category,
                Price = inputDto.Price,
                Quantity = inputDto.Quantity,
                ImageUrl = inputDto.ImageUrl,
                Rating = new ProductRating
                {
                    Rate = inputDto.Rating.Rate,
                    Count = inputDto.Rating.Count
                }
            };

            _Context.Products.Add(newProduct);
            await _Context.SaveChangesAsync();

            var response = new ProductResponseDto
            {
                Id = newProduct.Id,
                Title = newProduct.Title,
                Description = newProduct.Description,
                Category = newProduct.Category,
                Price = newProduct.Price,
                Quantity = newProduct.Quantity,
                ImageUrl = newProduct.ImageUrl,
                Rating = newProduct.Rating
            };

            return CreatedAtAction(nameof(GetProductById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateProduct(int id, ProductCreateUpdateDto updatedDto)
        {
            if (updatedDto is null)
                return BadRequest();

            var product = await _Context.Products.FindAsync(id);
            if (product is null)
                return NotFound();

            product.Title = updatedDto.Title;
            product.Description = updatedDto.Description;
            product.Category = updatedDto.Category;
            product.Price = updatedDto.Price;
            product.Quantity = updatedDto.Quantity;
            product.ImageUrl = updatedDto.ImageUrl;
            product.Rating.Rate = updatedDto.Rating.Rate;
            product.Rating.Count = updatedDto.Rating.Count;

            await _Context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _Context.Products.FindAsync(id);
            if (product is null)
                return NotFound();

            _Context.Products.Remove(product);
            await _Context.SaveChangesAsync();

            return NoContent();
        }
    }
}