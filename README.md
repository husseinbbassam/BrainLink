# BrainLink

An AI-powered Semantic Search API built with .NET 9, Microsoft Semantic Kernel, and Qdrant Vector Database.

## Features

- **Semantic Search**: Use embeddings to find semantically similar content
- **Hybrid Search**: Combine vector similarity with keyword matching for better accuracy
- **Text Chunking**: Automatically split large documents into smaller, overlapping segments
- **Document Ingestion**: Upload and process text documents
- **Vector Database**: Powered by Qdrant for efficient similarity search

## Architecture

The solution consists of three projects:

- **BrainLink.Api**: ASP.NET Core Web API with endpoints for ingestion and search
- **BrainLink.Core**: Domain models and abstractions
- **BrainLink.Infrastructure**: Implementations for Qdrant and OpenAI integration

## Prerequisites

- .NET 9 SDK
- Docker and Docker Compose
- OpenAI API Key (or Ollama for local embeddings)

## Getting Started

### 1. Start Qdrant Vector Database

```bash
docker-compose up -d
```

This will start Qdrant on:
- HTTP API: http://localhost:6333
- gRPC API: http://localhost:6334

### 2. Configure OpenAI API Key

Set your OpenAI API key as an environment variable:

```bash
# Linux/macOS
export OpenAI__ApiKey="your-openai-api-key"

# Windows PowerShell
$env:OpenAI__ApiKey="your-openai-api-key"
```

Or update `appsettings.Development.json`:

```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "EmbeddingModel": "text-embedding-3-small"
  }
}
```

### 3. Build and Run

```bash
# Build the solution
dotnet build

# Run the API
cd BrainLink.Api
dotnet run
```

The API will start on http://localhost:5000 (or https://localhost:5001)

## API Endpoints

### POST /ingest

Ingest text content into the system.

**Request Body:**
```json
{
  "content": "Your text content here...",
  "fileName": "document.txt",
  "metadata": {
    "author": "John Doe",
    "category": "Technology"
  }
}
```

**Response:**
```json
{
  "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Document ingested successfully"
}
```

### GET /search?query=your+search+query&limit=10

Perform semantic search using vector embeddings.

**Query Parameters:**
- `query` (required): Search query
- `limit` (optional): Number of results to return (default: 10)

**Response:**
```json
{
  "results": [
    {
      "chunkId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "content": "Matching content...",
      "score": 0.95,
      "metadata": {
        "file_name": "document.txt",
        "author": "John Doe"
      }
    }
  ],
  "totalResults": 10
}
```

### GET /search/hybrid?query=your+search+query&limit=10

Perform hybrid search combining vector similarity and keyword matching.

**Query Parameters:**
- `query` (required): Search query
- `limit` (optional): Number of results to return (default: 10)

**Response:** Same as `/search`

## Example Usage

### Using cURL

```bash
# Ingest a document
curl -X POST http://localhost:5000/ingest \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Artificial intelligence is transforming the world of technology. Machine learning algorithms can now perform complex tasks that were once thought to be exclusively human.",
    "fileName": "ai-intro.txt",
    "metadata": {
      "topic": "AI",
      "author": "Tech Writer"
    }
  }'

# Semantic search
curl "http://localhost:5000/search?query=machine%20learning&limit=5"

# Hybrid search
curl "http://localhost:5000/search/hybrid?query=artificial%20intelligence&limit=5"
```

### Using PowerShell

```powershell
# Ingest a document
$body = @{
    content = "Artificial intelligence is transforming the world..."
    fileName = "ai-intro.txt"
    metadata = @{
        topic = "AI"
        author = "Tech Writer"
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/ingest" -Method Post -Body $body -ContentType "application/json"

# Search
Invoke-RestMethod -Uri "http://localhost:5000/search?query=machine learning&limit=5"
```

## Configuration

### OpenAI Settings

Configure in `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "your-api-key",
    "EmbeddingModel": "text-embedding-3-small",
    "Endpoint": ""
  }
}
```

- `ApiKey`: Your OpenAI API key
- `EmbeddingModel`: The embedding model to use (default: text-embedding-3-small)
- `Endpoint`: Optional Azure OpenAI endpoint

### Qdrant Settings

```json
{
  "Qdrant": {
    "Host": "localhost",
    "Port": 6333,
    "CollectionName": "brainlink_documents",
    "VectorSize": 1536
  }
}
```

- `Host`: Qdrant server host
- `Port`: Qdrant HTTP API port
- `CollectionName`: Name of the collection to store vectors
- `VectorSize`: Dimension of embedding vectors (1536 for text-embedding-3-small)

## Technical Details

### Text Chunking

The system uses Semantic Kernel's `TextChunker` to split documents into:
- Maximum 512 tokens per chunk
- 50 tokens overlap between chunks
- Maintains context across chunk boundaries

### Hybrid Search

The hybrid search feature combines:
1. **Vector Search**: Finds semantically similar content using embeddings
2. **Keyword Search**: Uses Qdrant's full-text search for exact matches
3. **Score Fusion**: Boosts items found in both searches for better accuracy

### Embeddings

By default, the system uses OpenAI's `text-embedding-3-small` model:
- 1536 dimensions
- Cost-effective
- High quality semantic representations

## Development

### Project Structure

```
BrainLink/
├── BrainLink.Api/              # Web API
│   ├── Models/                 # Request/Response DTOs
│   └── Program.cs              # API configuration
├── BrainLink.Core/             # Domain layer
│   ├── Abstractions/           # Interfaces
│   └── Models/                 # Domain models
├── BrainLink.Infrastructure/   # Infrastructure layer
│   ├── Configuration/          # Settings classes
│   └── Services/               # Service implementations
└── docker-compose.yml          # Qdrant container
```

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

## License

MIT License

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
