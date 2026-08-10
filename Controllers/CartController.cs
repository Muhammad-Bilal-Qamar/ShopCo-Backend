//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using System.Security.Claims;
//using ShopCoAPI.Data;
//using ShopCoAPI.Models;
//using static ShopCoAPI.DTO.CartDTO;
//using static ShopCoAPI.DTO.CartItemDTO;

//namespace ShopCoAPI.Controllers
//{
//[Route("api/[controller]")]
//[Route("api/carts")]
//[ApiController]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//    public class CartController : ControllerBase
//    {
//        private readonly ShopCoDBContext _Context;

//        public CartController(ShopCoDBContext context)
//        {
//            _Context = context;
//        }

//        [HttpGet("user/{userId}")]
//        [HttpGet("{userId}")]
//        public async Task<ActionResult<CartResponseDto>> GetCartByUserId(int userId)
//        {
//            // ensure the caller is the same user (prevent access to other users' carts)
//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var callerId))
//                return Unauthorized();

//            if (callerId != userId)
//                return Forbid();

//            var userExists = await _Context.Users.AnyAsync(u => u.Id == userId);
//            if (!userExists)
//                return NotFound();

//            var cart = await _Context.Carts
//                .Include(c => c.Items)
//                .FirstOrDefaultAsync(c => c.UserId == userId);

//            if (cart is null)
//            {
//                cart = new Cart { UserId = userId };
//                _Context.Carts.Add(cart);
//                await _Context.SaveChangesAsync();
//            }

//            var productIds = cart.Items.Select(i => i.ProductId).ToList();
//            var products = await _Context.Products
//                .Where(p => productIds.Contains(p.Id))
//                .ToDictionaryAsync(p => p.Id);

//            var itemResponses = cart.Items.Select(item =>
//            {
//                products.TryGetValue(item.ProductId, out var product);

//                if (product is null)
//                {
//                    return new CartItemResponseDto
//                    {
//                        Id = item.Id,
//                        ProductId = item.ProductId,
//                        ProductTitle = "Out of Stock / Unavailable",
//                        ProductPrice = 0,
//                        ProductImageUrl = "No image available",
//                        Quantity = item.Quantity,
//                        IsAvailable = false
//                    };
//                }

//                return new CartItemResponseDto
//                {
//                    Id = item.Id,
//                    ProductId = item.ProductId,
//                    ProductTitle = product.Title,
//                    ProductPrice = product.Price,
//                    ProductImageUrl = product.ImageUrl,
//                    Quantity = item.Quantity,
//                    IsAvailable = true
//                };
//            }).ToList();

//            var response = new CartResponseDto
//            {
//                Id = cart.Id,
//                UserId = cart.UserId,
//                Items = itemResponses
//            };

//            return Ok(response);
//        }

//        [HttpPost("user/{userId}/add")]
//        [HttpPost("{userId}/add")]
//        public async Task<IActionResult> AddToCart(int userId, CartItemRequestDto requestDto)
//        {
//            // ensure caller matches route userId
//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var callerId))
//                return Unauthorized();

//            if (callerId != userId)
//                return Forbid();

//            if (requestDto is null || requestDto.Quantity <= 0)
//                return BadRequest();

//            var productExists = await _Context.Products.AnyAsync(p => p.Id == requestDto.ProductId);
//            if (!productExists)
//                return NotFound();

//            var cart = await _Context.Carts
//                .Include(c => c.Items)
//                .FirstOrDefaultAsync(c => c.UserId == userId);

//            if (cart is null)
//            {
//                cart = new Cart { UserId = userId };
//                _Context.Carts.Add(cart);
//                await _Context.SaveChangesAsync();
//            }

//            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == requestDto.ProductId);

//            if (existingItem is not null)
//            {

//                existingItem.Quantity += requestDto.Quantity;
//            }
//            else
//            {

//                var newItem = new CartItem
//                {
//                    CartId = cart.Id,
//                    ProductId = requestDto.ProductId,
//                    Quantity = requestDto.Quantity
//                };
//                _Context.CartItems.Add(newItem);
//            }

//            await _Context.SaveChangesAsync();
//            return Ok();
//        }

//        [HttpGet("me")]
//        public async Task<ActionResult<CartResponseDto>> GetMyCart()
//        {
//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var callerId))
//                return Unauthorized();

//            return await GetCartByUserId(callerId);
//        }

//        [HttpPut("items/{cartItemId}")]
//        public async Task<IActionResult> UpdateItemQuantity(int cartItemId, [FromBody] int newQuantity)
//        {
//            if (newQuantity <= 0)
//                return BadRequest();

//            var cartItem = await _Context.CartItems.FindAsync(cartItemId);
//            if (cartItem is null)
//                return NotFound();

//            cartItem.Quantity = newQuantity;
//            await _Context.SaveChangesAsync();

//            return NoContent();
//        }

//        [HttpDelete("items/{cartItemId}")]
//        public async Task<IActionResult> RemoveFromCart(int cartItemId)
//        {
//            var cartItem = await _Context.CartItems.FindAsync(cartItemId);
//            if (cartItem is null)
//                return NotFound();

//            _Context.CartItems.Remove(cartItem);
//            await _Context.SaveChangesAsync();

//            return NoContent();
//        }
//    }
//}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using ShopCoAPI.Data;
using ShopCoAPI.Models;
using static ShopCoAPI.DTO.CartDTO;
using static ShopCoAPI.DTO.CartItemDTO;

namespace ShopCoAPI.Controllers
{
    [Route("api/[controller]")]
    [Route("api/carts")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CartController : ControllerBase
    {
        private readonly ShopCoDBContext _Context;

        public CartController(ShopCoDBContext context)
        {
            _Context = context;
        }

        [HttpGet("user/{userId}")]
        [HttpGet("{userId}")]
        public async Task<ActionResult<CartResponseDto>> GetCartByUserId(int userId)
        {
            // ensure the caller is the same user (prevent access to other users' carts)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var callerId))
                return Unauthorized();

            if (callerId != userId)
                return Forbid();

            var userExists = await _Context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return NotFound();

            var cart = await _Context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null)
            {
                cart = new Cart { UserId = userId };
                _Context.Carts.Add(cart);
                await _Context.SaveChangesAsync();
            }

            var productIds = cart.Items.Select(i => i.ProductId).ToList();
            var products = await _Context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var itemResponses = cart.Items.Select(item =>
            {
                products.TryGetValue(item.ProductId, out var product);

                if (product is null)
                {
                    return new CartItemResponseDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductTitle = "Out of Stock / Unavailable",
                        ProductPrice = 0,
                        ProductImageUrl = "No image available",
                        Quantity = item.Quantity,
                        IsAvailable = false
                    };
                }

                return new CartItemResponseDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductTitle = product.Title,
                    ProductPrice = product.Price,
                    ProductImageUrl = product.ImageUrl,
                    Quantity = item.Quantity,
                    IsAvailable = true
                };
            }).ToList();

            var response = new CartResponseDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = itemResponses
            };

            return Ok(response);
        }

        [HttpPost("user/{userId}/add")]
        [HttpPost("{userId}/add")]
        public async Task<IActionResult> AddToCart(int userId, CartItemRequestDto requestDto)
        {
            // ensure caller matches route userId
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var callerId))
                return Unauthorized();

            if (callerId != userId)
                return Forbid();

            if (requestDto is null || requestDto.Quantity <= 0)
                return BadRequest();

            var productExists = await _Context.Products.AnyAsync(p => p.Id == requestDto.ProductId);
            if (!productExists)
                return NotFound();

            var cart = await _Context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null)
            {
                cart = new Cart { UserId = userId };
                _Context.Carts.Add(cart);
                await _Context.SaveChangesAsync();
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == requestDto.ProductId);

            if (existingItem is not null)
            {

                existingItem.Quantity += requestDto.Quantity;
            }
            else
            {

                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = requestDto.ProductId,
                    Quantity = requestDto.Quantity
                };
                _Context.CartItems.Add(newItem);
            }

            await _Context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("me")]
        public async Task<ActionResult<CartResponseDto>> GetMyCart()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var callerId))
                return Unauthorized();

            return await GetCartByUserId(callerId);
        }

        [HttpPut("items/{cartItemId}")]
        public async Task<IActionResult> UpdateItemQuantity(int cartItemId, [FromBody] int newQuantity)
        {
            if (newQuantity <= 0)
                return BadRequest();

            var cartItem = await _Context.CartItems.FindAsync(cartItemId);
            if (cartItem is null)
                return NotFound();

            cartItem.Quantity = newQuantity;
            await _Context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _Context.CartItems.FindAsync(cartItemId);
            if (cartItem is null)
                return NotFound();

            _Context.CartItems.Remove(cartItem);
            await _Context.SaveChangesAsync();

            return NoContent();
        }
    }
}