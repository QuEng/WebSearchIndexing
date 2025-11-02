namespace WebSearchIndexing.Modules.Reporting.Application.DTOs;

public sealed record DashboardStatsDto
{
    public ServiceAccountStatsDto ServiceAccounts { get; init; } = new();
    public QuotaStatsDto Quota { get; init; } = new();
    public UrlRequestStatsDto PendingRequests { get; init; } = new();
    public UrlRequestStatsDto CompletedRequests { get; init; } = new();
    public UrlRequestStatsDto CompletedRequestsToday { get; init; } = new();
    public bool IsServiceEnabled { get; init; }
}

public sealed record ServiceAccountStatsDto
{
    public int Count { get; init; }
}

public sealed record QuotaStatsDto
{
    public int TotalQuotaByServiceAccounts { get; init; }
    public int QuotaBySettings { get; init; }
    public int AvailableToday { get; init; }
}

public sealed record UrlRequestStatsDto
{
    public int Total { get; init; }
    public int Updated { get; init; }
    public int Deleted { get; init; }
    public int Rejected { get; init; }
}
