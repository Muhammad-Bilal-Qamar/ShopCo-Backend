namespace ShopCoAPI.Models
{
    public class Chat
    {
        public int ChatId { get; set; }
        
        public int UserId { get; set; }

        public Users? User { get; set; }

        public List<ChatMessage> Messages { get; set; } = new();
    }
}
