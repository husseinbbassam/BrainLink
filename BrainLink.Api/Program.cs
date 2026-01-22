using BrainLink.Api.Models;
using BrainLink.Core.Abstractions;
using BrainLink.Infrastructure.Configuration;
using BrainLink.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Configure settings
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<QdrantSettings>(builder.Configuration.GetSection("Qdrant"));

// Register services
builder.Services.AddSingleton<IEmbeddingService, SemanticKernelEmbeddingService>();
builder.Services.AddSingleton<IVectorStore, QdrantVectorStore>();
builder.Services.AddScoped<IIngestionService, IngestionService>();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize Qdrant collection on startup
using (var scope = app.Services.CreateScope())
{
    var vectorStore = scope.ServiceProvider.GetRequiredService<IVectorStore>();
    await vectorStore.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();

// POST /ingest - Ingest text content
app.MapPost("/ingest", async (IngestRequest request, IIngestionService ingestionService) =>
{
    if (string.IsNullOrWhiteSpace(request.Content))
    {
        return Results.BadRequest(new { error = "Content is required" });
    }

    if (string.IsNullOrWhiteSpace(request.FileName))
    {
        return Results.BadRequest(new { error = "FileName is required" });
    }

    var documentId = await ingestionService.IngestTextAsync(
        request.Content,
        request.FileName,
        request.Metadata);

    return Results.Ok(new IngestResponse
    {
        DocumentId = documentId,
        Message = "Document ingested successfully"
    });
})
.WithName("IngestText")
.WithOpenApi();

// GET /search - Semantic search
app.MapGet("/search", async (
    string query,
    int limit,
    IEmbeddingService embeddingService,
    IVectorStore vectorStore) =>
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return Results.BadRequest(new { error = "Query parameter is required" });
    }

    if (limit <= 0)
    {
        limit = 10;
    }

    // Generate embedding for the query
    var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(query);

    // Perform semantic search
    var results = await vectorStore.SearchAsync(queryEmbedding, limit);

    var response = new SearchResponse
    {
        Results = results.Select(r => new SearchResultDto
        {
            ChunkId = r.ChunkId,
            DocumentId = r.DocumentId,
            Content = r.Content,
            Score = r.Score,
            Metadata = r.Metadata
        }).ToList(),
        TotalResults = results.Count
    };

    return Results.Ok(response);
})
.WithName("Search")
.WithOpenApi();

// GET /search/hybrid - Hybrid search combining vector and keyword search
app.MapGet("/search/hybrid", async (
    string query,
    int limit,
    IEmbeddingService embeddingService,
    IVectorStore vectorStore) =>
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return Results.BadRequest(new { error = "Query parameter is required" });
    }

    if (limit <= 0)
    {
        limit = 10;
    }

    // Generate embedding for the query
    var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(query);

    // Perform hybrid search
    var results = await vectorStore.HybridSearchAsync(queryEmbedding, query, limit);

    var response = new SearchResponse
    {
        Results = results.Select(r => new SearchResultDto
        {
            ChunkId = r.ChunkId,
            DocumentId = r.DocumentId,
            Content = r.Content,
            Score = r.Score,
            Metadata = r.Metadata
        }).ToList(),
        TotalResults = results.Count
    };

    return Results.Ok(response);
})
.WithName("HybridSearch")
.WithOpenApi();

app.Run();
