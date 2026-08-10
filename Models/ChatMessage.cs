using System.ComponentModel.DataAnnotations;

namespace ShopCoAPI.Models
{
    public class ChatMessage
    {
        [Key]
        public int MessageId { get; set; }

        public int ChatId { get; set; }

        public int SenderId { get; set; }

        public int ReceiverId { get; set; }

        public string Message { get; set; }

        public string MessageStatus { get; set; }

        public DateTime TimeOfMessage { get; set; }

        public Chat? Chat { get; set; }
    }
}
