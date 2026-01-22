namespace BrainLink.Api.Models;

/// <summary>
/// Response model for search operation
/// </summary>
public class SearchResponse
{
    public List<SearchResultDto> Results { get; set; } = new();
    public int TotalResults { get; set; }
}

public class SearchResultDto
{
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public double Score { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
