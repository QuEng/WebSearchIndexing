using WebSearchIndexing.Modules.Reporting.Application.DTOs;

namespace WebSearchIndexing.Modules.Reporting.Application.Queries;

public interface IReportingQueryService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<PeriodStatsDto> GetPeriodStatsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<QuotaUsageDto> GetQuotaUsageAsync(DateTime? date = null, CancellationToken cancellationToken = default);
}
