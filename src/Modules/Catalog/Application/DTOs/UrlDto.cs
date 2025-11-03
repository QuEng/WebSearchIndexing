namespace WebSearchIndexing.Modules.Catalog.Application.DTOs;

public class UrlDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
}
