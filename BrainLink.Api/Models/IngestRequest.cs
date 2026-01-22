namespace BrainLink.Api.Models;

/// <summary>
/// Request model for text ingestion
/// </summary>
public class IngestRequest
{
    public string Content { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}
