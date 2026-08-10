using ShopCoAPI.Models;

namespace ShopCoAPI.DTOs
{
    public class ProductCreateUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }

        public string ImageUrl { get; set; } = "No image available";

        public ProductRating Rating { get; set; } = new();
    }

    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = "No image available";
        public ProductRating Rating { get; set; } = new();
    }
}