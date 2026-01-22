namespace BrainLink.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for OpenAI service
/// </summary>
public class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    public string Endpoint { get; set; } = string.Empty; // Optional for Azure OpenAI
}
