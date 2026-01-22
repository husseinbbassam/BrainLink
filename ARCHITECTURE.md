# BrainLink Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         Client Applications                      │
│                    (curl, Postman, Web Apps)                     │
└────────────────────┬────────────────────────────────────────────┘
                     │ HTTP/HTTPS
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                        BrainLink.Api                             │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Minimal API Endpoints                                     │  │
│  │  • POST /ingest      - Document ingestion                 │  │
│  │  • GET /search       - Semantic vector search             │  │
│  │  • GET /search/hybrid - Hybrid search (vector + keyword)  │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                       BrainLink.Core                             │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Domain Models                                            │  │
│  │  • Document          • DocumentChunk    • SearchResult   │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Abstractions                                             │  │
│  │  • IEmbeddingService  • IVectorStore  • IIngestionService│  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                   BrainLink.Infrastructure                       │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Services                                                 │  │
│  │  • SemanticKernelEmbeddingService                        │  │
│  │  • QdrantVectorStore                                     │  │
│  │  • IngestionService (with TextChunker)                   │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────┬──────────────────────────────┬───────────────────┘
              │                              │
              ▼                              ▼
┌──────────────────────────┐    ┌──────────────────────────┐
│    OpenAI API            │    │    Qdrant Vector DB      │
│                          │    │                          │
│  • text-embedding-3-small│    │  • Vector Storage        │
│  • 1536 dimensions       │    │  • Cosine Similarity     │
│  • High-quality embeddings│    │  • Full-text Search     │
└──────────────────────────┘    └──────────────────────────┘
         External                      Docker Container
```

## Data Flow

### 1. Document Ingestion Flow

```
User Request (POST /ingest)
    ↓
[API Controller]
    ↓
[IIngestionService]
    ↓
[TextChunker] ──→ Split into overlapping chunks
    ↓
[IEmbeddingService] ──→ Generate embeddings via Semantic Kernel
    ↓                   ↓
[Semantic Kernel] ──→ [OpenAI API] (text-embedding-3-small)
    ↓
[IVectorStore]
    ↓
[Qdrant Client] ──→ Store chunks + embeddings + metadata
    ↓
[Qdrant Database]
```

### 2. Semantic Search Flow

```
User Request (GET /search?query=...)
    ↓
[API Controller]
    ↓
[IEmbeddingService] ──→ Generate query embedding
    ↓                   ↓
[Semantic Kernel] ──→ [OpenAI API]
    ↓
[IVectorStore]
    ↓
[Qdrant Client] ──→ Vector similarity search (cosine distance)
    ↓
[Qdrant Database] ──→ Return top N similar chunks
    ↓
[API Response] ──→ Format results with scores and metadata
```

### 3. Hybrid Search Flow

```
User Request (GET /search/hybrid?query=...)
    ↓
[API Controller]
    ↓
[IEmbeddingService] ──→ Generate query embedding
    ↓
[IVectorStore]
    ├─→ Vector Search ──→ Qdrant similarity search
    │                     ↓
    │                   Results Set A (with scores)
    │
    └─→ Keyword Search ──→ Qdrant full-text search
                          ↓
                        Results Set B (with default scores)
    ↓
[Result Fusion] ──→ Combine & deduplicate
    │              Boost items in both sets
    ↓
[API Response] ──→ Top N results sorted by score
```

## Component Details

### BrainLink.Api
- **Technology**: ASP.NET Core 9 Minimal APIs
- **Responsibility**: HTTP endpoints, request validation, response formatting
- **Configuration**: appsettings.json with OpenAI and Qdrant settings
- **Features**: CORS support, OpenAPI documentation

### BrainLink.Core
- **Technology**: .NET 9 Class Library
- **Responsibility**: Domain models and service abstractions
- **Patterns**: Repository pattern, Dependency Injection
- **Models**: 
  - `Document`: Source document with metadata
  - `DocumentChunk`: Text chunk with embedding vector
  - `SearchResult`: Search result with similarity score

### BrainLink.Infrastructure
- **Technology**: .NET 9 Class Library with external dependencies
- **Responsibility**: Implementation of core abstractions
- **Dependencies**:
  - Microsoft.SemanticKernel (v1.x)
  - Qdrant.Client (v1.16.x)
- **Services**:
  - `SemanticKernelEmbeddingService`: Wraps Semantic Kernel for embeddings
  - `QdrantVectorStore`: Implements vector operations with Qdrant
  - `IngestionService`: Orchestrates chunking and embedding generation

## Key Technologies

| Component | Technology | Purpose |
|-----------|-----------|---------|
| Runtime | .NET 9 | Modern C# features, performance |
| API Framework | ASP.NET Core Minimal APIs | Lightweight HTTP endpoints |
| Embeddings | Microsoft Semantic Kernel | AI orchestration layer |
| LLM Provider | OpenAI API | Text embedding generation |
| Vector Database | Qdrant | Vector storage and similarity search |
| Container | Docker Compose | Infrastructure as code |

## Text Chunking Strategy

```
Original Document (e.g., 5000 tokens)
    ↓
[TextChunker.SplitPlainTextLines]
    ↓
Lines (max 512 tokens each)
    ↓
[TextChunker.SplitPlainTextParagraphs]
    ↓
Chunks with overlap:
┌────────────────┐
│   Chunk 1      │ (512 tokens)
│   (0-512)      │
└────────────────┘
      └─────┬─────┘ 50 token overlap
           ┌────────────────┐
           │   Chunk 2      │ (512 tokens)
           │   (462-974)    │
           └────────────────┘
                 └─────┬─────┘ 50 token overlap
                      ┌────────────────┐
                      │   Chunk 3      │
                      │   (924-1436)   │
                      └────────────────┘
```

**Benefits**:
- Maintains context across chunk boundaries
- Prevents information loss at split points
- Improves semantic search accuracy

## Hybrid Search Scoring

```
Vector Search Results:          Keyword Search Results:
┌─────────────────────┐         ┌─────────────────────┐
│ Chunk A: 0.95       │         │ Chunk A: 0.50       │
│ Chunk B: 0.87       │         │ Chunk C: 0.50       │
│ Chunk D: 0.82       │         │ Chunk E: 0.50       │
└─────────────────────┘         └─────────────────────┘
         │                               │
         └───────────┬───────────────────┘
                     ↓
            Combined Results:
         ┌─────────────────────┐
         │ Chunk A: 1.25*      │ ← Found in both (+0.30 boost)
         │ Chunk B: 0.87       │ ← Vector only
         │ Chunk D: 0.82       │ ← Vector only
         │ Chunk C: 0.50       │ ← Keyword only
         │ Chunk E: 0.50       │ ← Keyword only
         └─────────────────────┘
                     ↓
              Top N Results
```

## Security Considerations

- ✅ No hardcoded secrets (environment variables or configuration)
- ✅ API key validation before service usage
- ✅ Input validation on all endpoints
- ✅ No SQL injection risk (Qdrant uses its own query language)
- ✅ CORS configured for development (should be restricted in production)
- ⚠️ No authentication/authorization (add before production deployment)
- ⚠️ Rate limiting not implemented (consider for production)

## Scalability Considerations

### Current Architecture
- Single instance of API
- Single Qdrant container
- Synchronous embedding generation

### Production Recommendations
1. **Horizontal Scaling**: Deploy multiple API instances behind a load balancer
2. **Qdrant Clustering**: Use Qdrant distributed mode for high availability
3. **Caching**: Add Redis for caching frequent queries and embeddings
4. **Async Processing**: Queue ingestion requests for background processing
5. **Batch Operations**: Group embedding generation for better throughput
6. **Monitoring**: Add Application Insights or similar for observability

## Configuration

### appsettings.json
```json
{
  "OpenAI": {
    "ApiKey": "sk-...",
    "EmbeddingModel": "text-embedding-3-small",
    "Endpoint": ""  // Optional for Azure OpenAI
  },
  "Qdrant": {
    "Host": "localhost",
    "Port": 6333,
    "CollectionName": "brainlink_documents",
    "VectorSize": 1536
  }
}
```

### Environment Variables
- `OpenAI__ApiKey`: OpenAI API key (recommended for secrets)
- `OpenAI__Endpoint`: Azure OpenAI endpoint (optional)
- `Qdrant__Host`: Qdrant host (default: localhost)
- `Qdrant__Port`: Qdrant port (default: 6333)

## Performance Metrics

### Embedding Generation
- **Model**: text-embedding-3-small
- **Dimensions**: 1536
- **Latency**: ~100-300ms per request (depends on text length)
- **Cost**: $0.00002 per 1K tokens

### Vector Search
- **Engine**: Qdrant HNSW algorithm
- **Distance**: Cosine similarity
- **Latency**: <10ms for typical collections (<1M vectors)
- **Scalability**: Millions of vectors per collection

### Text Chunking
- **Algorithm**: Semantic Kernel TextChunker
- **Chunk Size**: 512 tokens (~2000 characters)
- **Overlap**: 50 tokens (~200 characters)
- **Processing**: In-memory, fast (<1ms per chunk)
