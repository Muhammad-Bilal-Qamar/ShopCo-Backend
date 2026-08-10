namespace ShopCoAPI.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; } = 0;

        public Cart? Cart { get; set; }

        public Products? Product { get; set; }
    }
}
