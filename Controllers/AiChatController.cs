using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopCoAPI.DTO;
using ShopCoAPI.Services.AiChat;
using System.Security.Claims;

namespace ShopCoAPI.Controllers
{
    [Route("api/aichat")]
    [ApiController]
    [Authorize] // Every action requires a valid JWT; role is re-checked per action below.
    public class AiChatController : ControllerBase
    {
        private const int MaxHistoryTurns = 12;

        private readonly IGroqChatService _groqChatService;
        private readonly IAiContextService _contextService;
        private readonly ILogger<AiChatController> _logger;

        public AiChatController(IGroqChatService groqChatService, IAiContextService contextService, ILogger<AiChatController> logger)
        {
            _groqChatService = groqChatService;
            _contextService = contextService;
            _logger = logger;
        }

        // 🙋 Customer AI assistant — scoped to active products + the caller's own cart.
        [HttpPost("customer")]
        public async Task<IActionResult> ChatAsCustomer([FromBody] AiChatRequestDTO request, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Message is required." });

            // Explicit boundary check: this endpoint never accepts a target user id from
            // the client — the JWT's own subject claim is the only source of "who am I".
            var dataContext = await _contextService.BuildCustomerContextAsync(userId);
            var systemPrompt = AiGuardrails.BuildCustomerSystemPrompt(dataContext);

            var history = TrimHistory(request.History)
                .Select(t => (t.Role, t.Content))
                .Append(("user", request.Message));

            var reply = await _groqChatService.GetCompletionAsync(systemPrompt, history, ct);
            var sanitized = AiGuardrails.SanitizeOutput(reply);

            return Ok(new AiChatResponseDTO { Reply = sanitized, Refused = sanitized == AiGuardrails.OffTopicRefusal });
        }

        // 👑 Admin AI assistant — full system context, admin role required.
        [HttpPost("admin")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ChatAsAdmin([FromBody] AiChatRequestDTO request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Message is required." });

            var dataContext = await _contextService.BuildAdminContextAsync();
            var systemPrompt = AiGuardrails.BuildAdminSystemPrompt(dataContext);

            var history = TrimHistory(request.History)
                .Select(t => (t.Role, t.Content))
                .Append(("user", request.Message));

            var reply = await _groqChatService.GetCompletionAsync(systemPrompt, history, ct);
            var sanitized = AiGuardrails.SanitizeOutput(reply);

            return Ok(new AiChatResponseDTO { Reply = sanitized, Refused = false });
        }

        private static List<AiChatTurnDTO> TrimHistory(List<AiChatTurnDTO>? history)
        {
            if (history == null || history.Count == 0) return new List<AiChatTurnDTO>();

            // Keep the payload small and ignore anything that isn't user/assistant.
            return history
                .Where(t => t.Role == "user" || t.Role == "assistant")
                .TakeLast(MaxHistoryTurns)
                .ToList();
        }
    }
}