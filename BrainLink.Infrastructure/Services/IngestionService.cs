using BrainLink.Core.Abstractions;
using BrainLink.Core.Models;
using Microsoft.SemanticKernel.Text;

#pragma warning disable SKEXP0050 // TextChunker is experimental

namespace BrainLink.Infrastructure.Services;

/// <summary>
/// Service for ingesting and chunking text documents
/// </summary>
public class IngestionService : IIngestionService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private const int MaxTokensPerChunk = 512;
    private const int OverlapTokens = 50;

    public IngestionService(IEmbeddingService embeddingService, IVectorStore vectorStore)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
    }

    public async Task<Guid> IngestTextAsync(
        string content, 
        string fileName, 
        Dictionary<string, string>? metadata = null, 
        CancellationToken cancellationToken = default)
    {
        // Create document
        var document = new Document
        {
            FileName = fileName,
            Content = content,
            Metadata = metadata ?? new Dictionary<string, string>()
        };

        // Chunk the text using Semantic Kernel's TextChunker
        var lines = TextChunker.SplitPlainTextLines(content, MaxTokensPerChunk);
        var paragraphs = TextChunker.SplitPlainTextParagraphs(lines, MaxTokensPerChunk, OverlapTokens);

        // Create document chunks
        var chunks = new List<DocumentChunk>();
        var chunkIndex = 0;

        foreach (var paragraph in paragraphs)
        {
            var chunk = new DocumentChunk
            {
                DocumentId = document.Id,
                Content = paragraph,
                ChunkIndex = chunkIndex++,
                Metadata = new Dictionary<string, string>(document.Metadata)
                {
                    ["file_name"] = fileName,
                    ["created_at"] = document.CreatedAt.ToString("O")
                }
            };
            chunks.Add(chunk);
        }

        // Generate embeddings for all chunks
        var texts = chunks.Select(c => c.Content).ToList();
        var embeddings = await _embeddingService.GenerateEmbeddingsAsync(texts, cancellationToken);

        // Assign embeddings to chunks
        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].Embedding = embeddings[i];
        }

        // Save chunks to vector store
        await _vectorStore.SaveChunksAsync(chunks, cancellationToken);

        return document.Id;
    }
}
