# BrainLink Quick Start Guide

This guide will help you get started with BrainLink in under 5 minutes.

## Prerequisites

Before you begin, ensure you have:
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) installed
- [Docker](https://www.docker.com/get-started) installed and running
- An [OpenAI API key](https://platform.openai.com/api-keys)

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

## Step 2: Configure OpenAI API Key

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
  "OpenAI": {
    "ApiKey": "sk-your-actual-api-key-here"
  }
}
```

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

### OpenAI API Key Error
If you see "API key not configured" or authentication errors:
1. Verify your API key is set correctly
2. Check the key has not expired
3. Ensure your OpenAI account has credits

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
