using BrainLink.Core.Abstractions;
using BrainLink.Core.Models;
using BrainLink.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace BrainLink.Infrastructure.Services;

/// <summary>
/// Implementation of vector store using Qdrant
/// </summary>
public class QdrantVectorStore : IVectorStore
{
    private readonly QdrantClient _client;
    private readonly QdrantSettings _settings;

    public QdrantVectorStore(IOptions<QdrantSettings> settings)
    {
        _settings = settings.Value;
        _client = new QdrantClient(host: _settings.Host, port: _settings.Port);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if collection exists
            var collections = await _client.ListCollectionsAsync(cancellationToken);
            var collectionExists = collections.Contains(_settings.CollectionName);

            if (!collectionExists)
            {
                // Create collection with vector configuration
                await _client.CreateCollectionAsync(
                    collectionName: _settings.CollectionName,
                    vectorsConfig: new VectorParams
                    {
                        Size = (ulong)_settings.VectorSize,
                        Distance = Distance.Cosine
                    },
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize Qdrant collection: {ex.Message}", ex);
        }
    }

    public async Task SaveChunksAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        var points = chunks.Select(chunk => new PointStruct
        {
            Id = new PointId { Uuid = chunk.Id.ToString() },
            Vectors = chunk.Embedding,
            Payload =
            {
                ["document_id"] = chunk.DocumentId.ToString(),
                ["content"] = chunk.Content,
                ["chunk_index"] = chunk.ChunkIndex,
                ["metadata"] = System.Text.Json.JsonSerializer.Serialize(chunk.Metadata)
            }
        }).ToList();

        await _client.UpsertAsync(
            collectionName: _settings.CollectionName,
            points: points,
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        float[] queryEmbedding, 
        int limit = 10, 
        CancellationToken cancellationToken = default)
    {
        var searchResult = await _client.SearchAsync(
            collectionName: _settings.CollectionName,
            vector: queryEmbedding,
            limit: (ulong)limit,
            cancellationToken: cancellationToken);

        return searchResult.Select(result => new SearchResult
        {
            ChunkId = Guid.Parse(result.Id.Uuid),
            DocumentId = Guid.Parse(result.Payload["document_id"].StringValue),
            Content = result.Payload["content"].StringValue,
            Score = result.Score,
            Metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                result.Payload["metadata"].StringValue) ?? new Dictionary<string, string>()
        }).ToList();
    }

    public async Task<IReadOnlyList<SearchResult>> HybridSearchAsync(
        float[] queryEmbedding, 
        string queryText, 
        int limit = 10, 
        CancellationToken cancellationToken = default)
    {
        // Perform vector search
        var vectorResults = await SearchAsync(queryEmbedding, limit * 2, cancellationToken);

        // Perform keyword search using Qdrant's scroll with filter
        var scrollResults = await _client.ScrollAsync(
            collectionName: _settings.CollectionName,
            filter: new Filter
            {
                Must =
                {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "content",
                            Match = new Match { Text = queryText }
                        }
                    }
                }
            },
            limit: (uint)limit,
            cancellationToken: cancellationToken);

        var keywordResults = scrollResults.Result.Select(point => new SearchResult
        {
            ChunkId = Guid.Parse(point.Id.Uuid),
            DocumentId = Guid.Parse(point.Payload["document_id"].StringValue),
            Content = point.Payload["content"].StringValue,
            Score = 0.5, // Default score for keyword matches
            Metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                point.Payload["metadata"].StringValue) ?? new Dictionary<string, string>()
        }).ToList();

        // Combine and deduplicate results
        var combinedResults = new Dictionary<Guid, SearchResult>();

        foreach (var result in vectorResults)
        {
            combinedResults[result.ChunkId] = result;
        }

        foreach (var result in keywordResults)
        {
            if (combinedResults.ContainsKey(result.ChunkId))
            {
                // Boost score for items found in both searches
                combinedResults[result.ChunkId].Score += 0.3;
            }
            else
            {
                combinedResults[result.ChunkId] = result;
            }
        }

        // Return top results sorted by score
        return combinedResults.Values
            .OrderByDescending(r => r.Score)
            .Take(limit)
            .ToList();
    }
}
