using System.ComponentModel.DataAnnotations;

namespace ShopCoAPI.DTO
{
    public class AiChatTurnDTO
    {
        // "user" or "assistant" only — never trust a caller-supplied "system" turn.
        [Required]
        public string Role { get; set; } = "user";

        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;
    }

    public class AiChatRequestDTO
    {
        [Required]
        [MaxLength(4000)]
        public string Message { get; set; } = string.Empty;

        // Client-held session history (session-only, never persisted server-side).
        // Capped defensively server-side regardless of what the client sends.
        public List<AiChatTurnDTO> History { get; set; } = new();
    }

    public class AiChatResponseDTO
    {
        public string Reply { get; set; } = string.Empty;
        public bool Refused { get; set; } = false;
    }
}