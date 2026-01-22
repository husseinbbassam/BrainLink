# BrainLink Quick Start Guide

This guide will help you get started with BrainLink in under 5 minutes.

## Prerequisites

Before you begin, ensure you have:
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) installed
- [Docker](https://www.docker.com/get-started) installed and running
- **Either:**
  - An [OpenAI API key](https://platform.openai.com/api-keys) (for cloud embeddings), **or**
  - [Ollama](https://ollama.ai) installed (for local embeddings)

## Step 1: Clone and Setup

```bash
# Clone the repository
git clone https://github.com/husseinbbassam/BrainLink.git
cd BrainLink

# Start Qdrant vector database
docker compose up -d

# Wait for Qdrant to be ready (about 5 seconds)
sleep 5
```

## Step 2: Configure Your Embedding Provider

Choose **Option A** (OpenAI - cloud) or **Option B** (Ollama - local):

### Option A: Using OpenAI (Cloud)

Set your OpenAI API key as an environment variable:

**Linux/macOS:**
```bash
export OpenAI__ApiKey="sk-your-actual-api-key-here"
```

**Windows PowerShell:**
```powershell
$env:OpenAI__ApiKey="sk-your-actual-api-key-here"
```

**Windows Command Prompt:**
```cmd
set OpenAI__ApiKey=sk-your-actual-api-key-here
```

Alternatively, update `appsettings.Development.json`:
```json
{
  "Embedding": {
    "Provider": "OpenAI"
  },
  "OpenAI": {
    "ApiKey": "sk-your-actual-api-key-here"
  }
}
```

### Option B: Using Ollama (Local)

1. **Install and start Ollama:**

Visit [ollama.ai](https://ollama.ai) for installation instructions, then:

```bash
# Start Ollama service
ollama serve

# In a new terminal, pull an embedding model
ollama pull mxbai-embed-large
```

2. **Configure BrainLink to use Ollama:**

Set environment variable:

**Linux/macOS:**
```bash
export Embedding__Provider="Ollama"
```

**Windows PowerShell:**
```powershell
$env:Embedding__Provider="Ollama"
```

**Windows Command Prompt:**
```cmd
set Embedding__Provider=Ollama
```

Alternatively, update `appsettings.Development.json`:
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

**Important:** When using Ollama, update `VectorSize` to match your model:
- `mxbai-embed-large`: 1024 dimensions
- `nomic-embed-text`: 768 dimensions
- `all-minilm`: 384 dimensions

## Step 3: Build and Run

```bash
# Build the solution
dotnet build

# Run the API
cd BrainLink.Api
dotnet run
```

The API will start at `http://localhost:5000` (or `https://localhost:5001` for HTTPS).

## Step 4: Test the API

Open a new terminal and run the test script:

```bash
# Make sure you're in the root directory
cd BrainLink

# Run the test script (requires jq for pretty JSON output)
./test-api.sh
```

Or test manually with curl:

### Ingest a Document

```bash
curl -X POST http://localhost:5000/ingest \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Artificial intelligence is revolutionizing technology through machine learning and deep learning techniques.",
    "fileName": "ai-basics.txt",
    "metadata": {
      "topic": "AI"
    }
  }'
```

### Search for Similar Content

```bash
curl "http://localhost:5000/search?query=machine%20learning&limit=3"
```

### Hybrid Search (Vector + Keyword)

```bash
curl "http://localhost:5000/search/hybrid?query=artificial%20intelligence&limit=3"
```

## Step 5: Explore the API

Visit the OpenAPI documentation at:
- `http://localhost:5000/openapi/v1.json` (JSON specification)
- Use tools like [Swagger UI](https://swagger.io/tools/swagger-ui/) to interact with the API

## Common Issues

### Qdrant Connection Error
If you see connection errors to Qdrant:
1. Check Docker is running: `docker ps`
2. Verify Qdrant is running: `docker compose ps`
3. Check Qdrant is accessible: `curl http://localhost:6333`

### Vector Size Mismatch
If you see "dimension mismatch" errors:
1. Ensure `Qdrant.VectorSize` matches your embedding model's dimensions
2. For OpenAI `text-embedding-3-small`: use 1536
3. For Ollama `mxbai-embed-large`: use 1024
4. For Ollama `nomic-embed-text`: use 768
5. If you need to change vector size, delete and recreate the collection:
   ```bash
   docker compose down -v  # This deletes all data
   docker compose up -d
   ```

### OpenAI API Key Error (when using OpenAI)
If you see "API key not configured" or authentication errors:
1. Verify your API key is set correctly
2. Check the key has not expired
3. Ensure your OpenAI account has credits

### Ollama Connection Error (when using Ollama)
If you see connection errors to Ollama:
1. Ensure Ollama is running: `ollama serve`
2. Check Ollama is accessible: `curl http://localhost:11434`
3. Verify the model is downloaded: `ollama list`
4. If model is missing, pull it: `ollama pull mxbai-embed-large`

### Port Already in Use
If port 5000 is already in use:
1. Stop other applications using the port
2. Or change the port in `Properties/launchSettings.json`

## Next Steps

- Read the full [README.md](README.md) for detailed documentation
- Explore the codebase architecture in the three projects
- Customize chunk sizes and embedding models in the configuration
- Integrate with your own data sources
- Add authentication and authorization for production use

## Cleanup

When you're done testing:

```bash
# Stop the API (Ctrl+C in the terminal running the API)

# Stop and remove Qdrant container
docker compose down

# Optional: Remove Qdrant data volume
docker compose down -v
```

## Support

For issues and questions:
- Check existing [GitHub Issues](https://github.com/husseinbbassam/BrainLink/issues)
- Review the main [README.md](README.md)
- Consult [Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
- Refer to [Qdrant Documentation](https://qdrant.tech/documentation/)
