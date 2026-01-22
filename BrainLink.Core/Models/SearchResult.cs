namespace BrainLink.Core.Models;

/// <summary>
/// Represents a search result with similarity score
/// </summary>
public class SearchResult
{
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public double Score { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
