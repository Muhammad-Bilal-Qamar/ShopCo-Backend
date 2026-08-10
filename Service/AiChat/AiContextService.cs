using Microsoft.EntityFrameworkCore;
using ShopCoAPI.Data;
using System.Text.Json;

namespace ShopCoAPI.Services.AiChat
{
    // CONTEXT BOUNDARY ENFORCEMENT lives here, not just in the prompt:
    // each method only ever queries the tables/rows a given role is allowed to see,
    // and only projects the fields the AI is allowed to know about (no internal IDs
    // beyond what's needed, no passwords, no other users' data for customers).
    public class AiContextService : IAiContextService
    {
        private readonly ShopCoDBContext _context;
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

        public AiContextService(ShopCoDBContext context)
        {
            _context = context;
        }

        public async Task<string> BuildCustomerContextAsync(int userId)
        {
            var products = await _context.Products
                .Where(p => p.Quantity >= 0) // "active" products
                .Select(p => new
                {
                    p.Title,
                    p.Category,
                    p.Description,
                    p.Price,
                    InStock = p.Quantity > 0
                })
                .Take(200) // keep prompt size bounded
                .ToListAsync();

            var cart = await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .Select(c => new
                {
                    Items = c.Items.Select(i => new
                    {
                        Product = i.Product != null ? i.Product.Title : "Unknown item",
                        i.Quantity,
                        UnitPrice = i.Product != null ? i.Product.Price : 0
                    })
                })
                .FirstOrDefaultAsync();

            var payload = new
            {
                role = "customer",
                activeProducts = products,
                myCart = cart?.Items ?? Enumerable.Empty<object>()
            };

            return JsonSerializer.Serialize(payload, JsonOptions);
        }

        public async Task<string> BuildAdminContextAsync()
        {
            var products = await _context.Products
                .Select(p => new { p.Title, p.Category, p.Price, p.Quantity })
                .Take(300)
                .ToListAsync();

            var users = await _context.Users
                .Select(u => new { u.Id, u.Name, u.Email, u.Role })
                .Take(300)
                .ToListAsync();

            var cartSummaries = await _context.Carts
                .Include(c => c.Items)
                .Select(c => new
                {
                    c.UserId,
                    ItemCount = c.Items.Count,
                    TotalUnits = c.Items.Sum(i => i.Quantity)
                })
                .ToListAsync();

            var metrics = new
            {
                totalUsers = users.Count,
                totalProducts = products.Count,
                totalActiveCarts = cartSummaries.Count(c => c.ItemCount > 0),
                outOfStockProducts = products.Count(p => p.Quantity <= 0)
            };

            var payload = new
            {
                role = "admin",
                systemMetrics = metrics,
                products,
                users,
                cartSummaries
            };

            return JsonSerializer.Serialize(payload, JsonOptions);
        }
    }
}