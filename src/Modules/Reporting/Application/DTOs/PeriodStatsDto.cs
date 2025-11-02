namespace WebSearchIndexing.Modules.Reporting.Application.DTOs;

public sealed record PeriodStatsDto
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public UrlRequestStatsDto CompletedRequests { get; init; } = new();
    public UrlRequestStatsDto FailedRequests { get; init; } = new();
}

public sealed record QuotaUsageDto
{
    public int TotalQuota { get; init; }
    public int UsedQuota { get; init; }
    public int AvailableQuota { get; init; }
    public DateTime Date { get; init; }
}
