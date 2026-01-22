namespace BrainLink.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for embedding service provider
/// </summary>
public class EmbeddingSettings
{
    /// <summary>
    /// The provider to use for embeddings: "OpenAI" or "Ollama"
    /// </summary>
    public string Provider { get; set; } = "OpenAI";
}
