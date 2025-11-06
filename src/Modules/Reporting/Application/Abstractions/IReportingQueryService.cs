using WebSearchIndexing.Modules.Reporting.Application.DTOs;

namespace WebSearchIndexing.Modules.Reporting.Application.Abstractions;

/// <summary>
/// Query service for reporting functionality
/// </summary>
public interface IReportingQueryService
{
    /// <summary>
    /// Gets dashboard statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard statistics DTO</returns>
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets statistics for a specific time period
    /// </summary>
    /// <param name="from">Start date of the period</param>
    /// <param name="to">End date of the period</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Period statistics DTO</returns>
    Task<PeriodStatsDto> GetPeriodStatsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets quota usage information
    /// </summary>
    /// <param name="date">Specific date for quota usage, null for current date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Quota usage DTO</returns>
    Task<QuotaUsageDto> GetQuotaUsageAsync(DateTime? date = null, CancellationToken cancellationToken = default);
}
