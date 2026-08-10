namespace ShopCoAPI.Settings
{
    public class GroqSettings
    {
        // Never hardcode the real key here or in appsettings.json.
        // Set it via environment variable Groq__ApiKey, or:
        //   dotnet user-secrets set "Groq:ApiKey" "gsk_..."
        public string ApiKey { get; set; } = string.Empty;

        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/chat/completions";

        public string Model { get; set; } = "llama-3.3-70b-versatile";

        public int TimeoutSeconds { get; set; } = 30;

        public int MaxOutputTokens { get; set; } = 700;
    }
}