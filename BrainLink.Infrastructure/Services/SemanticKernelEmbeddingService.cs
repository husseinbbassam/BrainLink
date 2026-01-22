using BrainLink.Core.Abstractions;
using BrainLink.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

#pragma warning disable CS0618 // Type or member is obsolete - using deprecated API until migration

namespace BrainLink.Infrastructure.Services;

/// <summary>
/// Implementation of embedding service using Semantic Kernel
/// </summary>
public class SemanticKernelEmbeddingService : IEmbeddingService
{
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly OpenAISettings _settings;

    public SemanticKernelEmbeddingService(IOptions<OpenAISettings> settings)
    {
        _settings = settings.Value;
        
        // Build kernel with OpenAI embedding service
        var kernelBuilder = Kernel.CreateBuilder();
        
        if (!string.IsNullOrEmpty(_settings.Endpoint))
        {
            // Azure OpenAI
            kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
                deploymentName: _settings.EmbeddingModel,
                endpoint: _settings.Endpoint,
                apiKey: _settings.ApiKey);
        }
        else
        {
            // OpenAI
            kernelBuilder.AddOpenAITextEmbeddingGeneration(
                modelId: _settings.EmbeddingModel,
                apiKey: _settings.ApiKey);
        }
        
        var kernel = kernelBuilder.Build();
        _embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var embedding = await _embeddingService.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
        return embedding.ToArray();
    }

    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        var embeddings = new List<float[]>();
        
        foreach (var text in texts)
        {
            var embedding = await GenerateEmbeddingAsync(text, cancellationToken);
            embeddings.Add(embedding);
        }
        
        return embeddings;
    }
}
