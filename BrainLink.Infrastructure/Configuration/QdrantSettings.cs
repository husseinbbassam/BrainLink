namespace BrainLink.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Qdrant vector database
/// </summary>
public class QdrantSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6333;
    public string CollectionName { get; set; } = "brainlink_documents";
    public int VectorSize { get; set; } = 1536; // Default for text-embedding-3-small
}
