using BrainLink.Core.Models;

namespace BrainLink.Core.Abstractions;

/// <summary>
/// Service for chunking and ingesting text documents
/// </summary>
public interface IIngestionService
{
    /// <summary>
    /// Ingests text content by chunking, generating embeddings, and storing in vector database
    /// </summary>
    Task<Guid> IngestTextAsync(string content, string fileName, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
}
