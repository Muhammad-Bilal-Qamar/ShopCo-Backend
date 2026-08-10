namespace ShopCoAPI.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public Users? User { get; set; }

        public List<CartItem> Items { get; set; } = new();
    }
}
