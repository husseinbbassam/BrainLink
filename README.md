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

You can use either **OpenAI** (cloud-based) or **Ollama** (local) for embeddings.

### 1. Start Qdrant Vector Database

```bash
docker-compose up -d
```

This will start Qdrant on:
- HTTP API: http://localhost:6333
- gRPC API: http://localhost:6334

### 2. Choose Your Embedding Provider

#### Option A: Using OpenAI (Cloud)

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
  "Embedding": {
    "Provider": "OpenAI"
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "EmbeddingModel": "text-embedding-3-small"
  }
}
```

#### Option B: Using Ollama (Local)

1. Install and run Ollama:

```bash
# Install Ollama (see https://ollama.ai for installation instructions)

# Start Ollama
ollama serve

# Pull an embedding model
ollama pull mxbai-embed-large
```

2. Configure BrainLink to use Ollama:

Set environment variables:

```bash
# Linux/macOS
export Embedding__Provider="Ollama"

# Windows PowerShell
$env:Embedding__Provider="Ollama"
```

Or update `appsettings.Development.json`:

```json
{
  "Embedding": {
    "Provider": "Ollama"
  },
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "EmbeddingModel": "mxbai-embed-large"
  },
  "Qdrant": {
    "VectorSize": 1024
  }
}
```

**Note:** Different embedding models have different vector dimensions:
- OpenAI `text-embedding-3-small`: 1536 dimensions
- Ollama `mxbai-embed-large`: 1024 dimensions
- Ollama `nomic-embed-text`: 768 dimensions

Make sure to update `Qdrant.VectorSize` to match your model's dimensions.

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

### Embedding Provider Selection

Configure in `appsettings.json`:

```json
{
  "Embedding": {
    "Provider": "OpenAI"
  }
}
```

- `Provider`: Choose `"OpenAI"` or `"Ollama"` (default: OpenAI)

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

### Ollama Settings

Configure in `appsettings.json`:

```json
{
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "EmbeddingModel": "mxbai-embed-large"
  }
}
```

- `Endpoint`: Ollama API endpoint (default: http://localhost:11434)
- `EmbeddingModel`: The embedding model to use (default: mxbai-embed-large)

**Recommended Ollama Embedding Models:**
- `mxbai-embed-large`: 1024 dimensions, high quality
- `nomic-embed-text`: 768 dimensions, fast and efficient
- `all-minilm`: 384 dimensions, lightweight

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
- `VectorSize`: Dimension of embedding vectors (must match your embedding model)

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

BrainLink supports two embedding providers:

#### OpenAI Embeddings (Cloud)
- `text-embedding-3-small`: 1536 dimensions, cost-effective, high quality
- `text-embedding-3-large`: 3072 dimensions, highest quality
- `text-embedding-ada-002`: 1536 dimensions, legacy model

#### Ollama Embeddings (Local)
- `mxbai-embed-large`: 1024 dimensions, excellent quality
- `nomic-embed-text`: 768 dimensions, fast and efficient
- `all-minilm`: 384 dimensions, lightweight for resource-constrained environments

**Advantages of Ollama:**
- 🔒 **Privacy**: All data stays local
- 💰 **Cost**: No API costs
- ⚡ **Speed**: No network latency
- 🌐 **Offline**: Works without internet

**Advantages of OpenAI:**
- 🎯 **Quality**: State-of-the-art embeddings
- 🚀 **Easy Setup**: No local infrastructure
- 📈 **Scalability**: Handles any load

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
