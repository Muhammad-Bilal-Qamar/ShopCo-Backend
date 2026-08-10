using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopCoAPI.Data;
using ShopCoAPI.Models;
using System.Security.Claims;

namespace ShopCoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires users to be logged in via JWT
    public class ChatsController : ControllerBase
    {
        private readonly ShopCoDBContext _context;

        public ChatsController(ShopCoDBContext context)
        {
            _context = context;
        }

        // 🙋 0. Get (or create) the current logged-in user's own chat, plus who the admin is
        [HttpGet("mine")]
        public async Task<IActionResult> GetOrCreateMyChat()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.UserId == userId);
            if (chat == null)
            {
                chat = new Chat { UserId = userId };
                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();
            }

            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");

            return Ok(new
            {
                chatId = chat.ChatId,
                userId = userId,
                adminId = admin?.Id
            });
        }

        // 📜 1. Get chat history between a specific user and the admin
        [HttpGet("history/{chatId}")]
        public async Task<IActionResult> GetChatHistory(int chatId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.TimeOfMessage)
                .ToListAsync();

            return Ok(messages);
        }

        // 👑 2. Admin Only: Get a list of all active chats with user details
        [HttpGet("active")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetActiveChats()
        {
            // NOTE: Chats.Max(m => m.TimeOfMessage) throws when a chat has zero
            // messages yet (e.g. a customer opened "Human Support" but never sent
            // anything) because MAX() over an empty group returns SQL NULL, which
            // cannot be materialized into the non-nullable DateTime TimeOfMessage.
            // That exception used to fail this whole endpoint, so the admin's chat
            // list stayed empty until a message came in and (incidentally) caused
            // a re-fetch that happened to succeed. Casting to DateTime? first lets
            // empty chats come back with a null LastMessageTime instead of blowing
            // up the query.
            var activeChats = await _context.Chats
                .Include(c => c.User) // Assuming Chat model has a virtual User property
                .Select(c => new
                {
                    c.ChatId,
                    c.UserId,
                    UserName = c.User != null ? c.User.Name : "Unknown User",
                    LastMessageTime = _context.ChatMessages
                        .Where(m => m.ChatId == c.ChatId)
                        .Max(m => (DateTime?)m.TimeOfMessage)
                })
                .OrderByDescending(c => c.LastMessageTime)
                .ToListAsync();

            return Ok(activeChats);
        }
    }
}