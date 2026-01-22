namespace BrainLink.Api.Models;

/// <summary>
/// Response model for ingestion operation
/// </summary>
public class IngestResponse
{
    public Guid DocumentId { get; set; }
    public string Message { get; set; } = string.Empty;
}
