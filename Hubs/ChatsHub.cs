using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ShopCoAPI.Data;
using ShopCoAPI.Models;

namespace ShopCoAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ShopCoDBContext _context;

        public ChatHub(ShopCoDBContext context)
        {
            _context = context;
        }

        public async Task SendMessage(int chatId, int senderId, int receiverId, string message)
        {
            var chatMessage = new ChatMessage
            {
                ChatId = chatId,
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                MessageStatus = "Sent",
                TimeOfMessage = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", senderId, message);
        }
    }
}