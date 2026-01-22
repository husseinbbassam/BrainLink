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

    public SemanticKernelEmbeddingService(
        IOptions<EmbeddingSettings> embeddingSettings,
        IOptions<OpenAISettings> openAISettings,
        IOptions<OllamaSettings> ollamaSettings)
    {
        var provider = embeddingSettings.Value.Provider;
        
        // Build kernel with the selected embedding service
        var kernelBuilder = Kernel.CreateBuilder();
        
        if (provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
        {
            // Ollama
            var ollamaConfig = ollamaSettings.Value;
            kernelBuilder.AddOllamaTextEmbeddingGeneration(
                modelId: ollamaConfig.EmbeddingModel,
                endpoint: new Uri(ollamaConfig.Endpoint));
        }
        else
        {
            // OpenAI (default)
            var openAIConfig = openAISettings.Value;
            
            if (!string.IsNullOrEmpty(openAIConfig.Endpoint))
            {
                // Azure OpenAI
                kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
                    deploymentName: openAIConfig.EmbeddingModel,
                    endpoint: openAIConfig.Endpoint,
                    apiKey: openAIConfig.ApiKey);
            }
            else
            {
                // OpenAI
                kernelBuilder.AddOpenAITextEmbeddingGeneration(
                    modelId: openAIConfig.EmbeddingModel,
                    apiKey: openAIConfig.ApiKey);
            }
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
