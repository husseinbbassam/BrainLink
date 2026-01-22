namespace BrainLink.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Ollama service
/// </summary>
public class OllamaSettings
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string EmbeddingModel { get; set; } = "mxbai-embed-large";
}
