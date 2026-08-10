namespace ShopCoAPI.Models
{
    public class Products
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public ProductRating Rating { get; set; } = new ProductRating();

        public double Price { get; set; }

        public string ImageUrl { get; set; }

        public int Quantity { get; set; }
    }
}
