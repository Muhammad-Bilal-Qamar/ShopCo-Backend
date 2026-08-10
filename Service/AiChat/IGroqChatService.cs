namespace ShopCoAPI.Services.AiChat
{
    public interface IGroqChatService
    {
        Task<string> GetCompletionAsync(string systemPrompt, IEnumerable<(string role, string content)> history, CancellationToken ct = default);
    }
}