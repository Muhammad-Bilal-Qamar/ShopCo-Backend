using Microsoft.EntityFrameworkCore;
using ShopCoAPI.Models;

namespace ShopCoAPI.Data
{
    public class ShopCoDBContext : DbContext
    {
        public DbSet<Products> Products { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Chat> Chats { get; set; } 
        public DbSet<ChatMessage> ChatMessages { get; set; } 

        public ShopCoDBContext(DbContextOptions<ShopCoDBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Products>()
                .ComplexProperty(p => p.Rating);
        }
    }
}