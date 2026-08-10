namespace ShopCoAPI.DTO
{
    public class CartItemDTO
    {
        public class CartItemRequestDto
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
        public class CartItemResponseDto
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public string ProductTitle { get; set; } = string.Empty;
            public double ProductPrice { get; set; }
            public string ProductImageUrl { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public bool IsAvailable { get; set; } = true;
            public double TotalPrice => IsAvailable ? (ProductPrice * Quantity) : 0;
        }
    }
}
