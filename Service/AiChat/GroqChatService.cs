using Microsoft.Extensions.Options;
using ShopCoAPI.Settings;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ShopCoAPI.Services.AiChat
{
    public class GroqChatService : IGroqChatService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqSettings _settings;
        private readonly ILogger<GroqChatService> _logger;

        public GroqChatService(HttpClient httpClient, IOptions<GroqSettings> settings, ILogger<GroqChatService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> GetCompletionAsync(string systemPrompt, IEnumerable<(string role, string content)> history, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                _logger.LogError("Groq API key is not configured. Set the Groq__ApiKey environment variable or a user-secret.");
                return "Support chat is temporarily unavailable. Please try again later or use Human Support.";
            }

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            foreach (var (role, content) in history)
            {
                // Defense-in-depth: never forward a "system" role from client-supplied history.
                var safeRole = role == "assistant" ? "assistant" : "user";
                messages.Add(new { role = safeRole, content });
            }

            var requestBody = new
            {
                model = _settings.Model,
                messages,
                max_tokens = _settings.MaxOutputTokens,
                temperature = 0.3
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_settings.TimeoutSeconds));

            try
            {
                var response = await _httpClient.SendAsync(request, cts.Token);
                var raw = await response.Content.ReadAsStringAsync(cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Groq API error {StatusCode}: {Body}", response.StatusCode, raw);
                    return "Sorry, the support assistant is having trouble responding right now. Please try again in a moment.";
                }

                using var doc = JsonDocument.Parse(raw);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return content ?? "Sorry, I couldn't generate a response. Please try again.";
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Groq API request timed out.");
                return "The assistant took too long to respond. Please try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Groq API.");
                return "Something went wrong talking to the support assistant. Please try again.";
            }
        }
    }
}