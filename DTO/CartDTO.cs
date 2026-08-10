namespace ShopCoAPI.DTO
{
    public class CartDTO
    {
        public class CartResponseDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public List<CartItemDTO.CartItemResponseDto> Items { get; set; } = new();
            public double CartSubTotal => Items.Sum(item => item.TotalPrice);
        }
    }
}
