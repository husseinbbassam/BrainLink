using BrainLink.Core.Models;

namespace BrainLink.Core.Abstractions;

/// <summary>
/// Repository for vector storage operations
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// Saves document chunks with their embeddings to the vector store
    /// </summary>
    Task SaveChunksAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Performs a similarity search using vector embeddings
    /// </summary>
    Task<IReadOnlyList<SearchResult>> SearchAsync(float[] queryEmbedding, int limit = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Performs hybrid search combining vector similarity and keyword matching
    /// </summary>
    Task<IReadOnlyList<SearchResult>> HybridSearchAsync(float[] queryEmbedding, string queryText, int limit = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Initializes the vector store collection/index
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
