namespace WebSearchIndexing.Modules.Catalog.Application.DTOs;

public class BatchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int TotalUrls { get; set; }
    public int ProcessedUrls { get; set; }
    public int SuccessfulUrls { get; set; }
    public int FailedUrls { get; set; }
    public string? ErrorMessage { get; set; }
}
