using WebSearchIndexing.Modules.Catalog.Application.Abstractions;
using WebSearchIndexing.Modules.Core.Application.Abstractions;
using WebSearchIndexing.Modules.Reporting.Application.DTOs;
using WebSearchIndexing.Modules.Reporting.Application.Abstractions;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Reporting.Application.Services;

internal sealed class ReportingQueryService : IReportingQueryService
{
    private readonly IServiceAccountRepository _serviceAccountRepository;
    private readonly IUrlRequestRepository _urlRequestRepository;
    private readonly ISettingsRepository _settingsRepository;

    public ReportingQueryService(
        IServiceAccountRepository serviceAccountRepository,
        IUrlRequestRepository urlRequestRepository,
        ISettingsRepository settingsRepository)
    {
        _serviceAccountRepository = serviceAccountRepository;
        _urlRequestRepository = urlRequestRepository;
        _settingsRepository = settingsRepository;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetAsync(cancellationToken);

        var serviceAccountsCount = await _serviceAccountRepository.GetCountAsync(cancellationToken);
        var quotaByServiceAccounts = await _serviceAccountRepository.GetQuotaByAllAsync(cancellationToken);
        var quotaAvailableToday = await _serviceAccountRepository.GetQuotaAvailableTodayAsync(cancellationToken);

        var pendingUrlRequestsCount = await _urlRequestRepository.GetRequestsCountAsync(UrlItemStatus.Pending, cancellationToken: cancellationToken);
        var updatedPendingUrlRequestsCount = await _urlRequestRepository.GetRequestsCountAsync(UrlItemStatus.Pending, UrlItemType.Updated, cancellationToken);
        var deletedPendingUrlRequestsCount = await _urlRequestRepository.GetRequestsCountAsync(UrlItemStatus.Pending, UrlItemType.Deleted, cancellationToken);

        var completedUrlRequestsCount = await _urlRequestRepository.GetRequestsCountAsync(UrlItemStatus.Completed, cancellationToken: cancellationToken);
        var updatedCompletedUrlRequestsCount = await _urlRequestRepository.GetRequestsCountAsync(UrlItemStatus.Completed, UrlItemType.Updated, cancellationToken);
        var deletedCompletedUrlRequestsCount = await _urlRequestRepository.GetRequestsCountAsync(UrlItemStatus.Completed, UrlItemType.Deleted, cancellationToken);
        var rejectedCompletedUrlRequestsCount = await _urlRequestRepository.GetRequestsCountAsync(UrlItemStatus.Failed, cancellationToken: cancellationToken);

        var completedUrlRequestsTodayCount = await _urlRequestRepository.GetRequestsCountAsync(TimeSpan.FromDays(1), UrlItemStatus.Completed, cancellationToken: cancellationToken);
        var updatedCompletedUrlRequestsTodayCount = await _urlRequestRepository.GetRequestsCountAsync(TimeSpan.FromDays(1), UrlItemStatus.Completed, UrlItemType.Updated, cancellationToken);
        var deletedCompletedUrlRequestsTodayCount = await _urlRequestRepository.GetRequestsCountAsync(TimeSpan.FromDays(1), UrlItemStatus.Completed, UrlItemType.Deleted, cancellationToken);
        var rejectedCompletedUrlRequestsTodayCount = await _urlRequestRepository.GetRequestsCountAsync(TimeSpan.FromDays(1), UrlItemStatus.Failed, cancellationToken: cancellationToken);

        return new DashboardStatsDto
        {
            IsServiceEnabled = settings.IsEnabled,
            ServiceAccounts = new ServiceAccountStatsDto
            {
                Count = serviceAccountsCount
            },
            Quota = new QuotaStatsDto
            {
                TotalQuotaByServiceAccounts = quotaByServiceAccounts,
                QuotaBySettings = settings.RequestsPerDay,
                AvailableToday = quotaAvailableToday
            },
            PendingRequests = new UrlRequestStatsDto
            {
                Total = pendingUrlRequestsCount,
                Updated = updatedPendingUrlRequestsCount,
                Deleted = deletedPendingUrlRequestsCount,
                Rejected = 0
            },
            CompletedRequests = new UrlRequestStatsDto
            {
                Total = completedUrlRequestsCount,
                Updated = updatedCompletedUrlRequestsCount,
                Deleted = deletedCompletedUrlRequestsCount,
                Rejected = rejectedCompletedUrlRequestsCount
            },
            CompletedRequestsToday = new UrlRequestStatsDto
            {
                Total = completedUrlRequestsTodayCount,
                Updated = updatedCompletedUrlRequestsTodayCount,
                Deleted = deletedCompletedUrlRequestsTodayCount,
                Rejected = rejectedCompletedUrlRequestsTodayCount
            }
        };
    }

    public async Task<PeriodStatsDto> GetPeriodStatsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var timeSpan = to - from;

        var completedCount = await _urlRequestRepository.GetRequestsCountAsync(timeSpan, UrlItemStatus.Completed, cancellationToken: cancellationToken);
        var updatedCompletedCount = await _urlRequestRepository.GetRequestsCountAsync(timeSpan, UrlItemStatus.Completed, UrlItemType.Updated, cancellationToken);
        var deletedCompletedCount = await _urlRequestRepository.GetRequestsCountAsync(timeSpan, UrlItemStatus.Completed, UrlItemType.Deleted, cancellationToken);

        var failedCount = await _urlRequestRepository.GetRequestsCountAsync(timeSpan, UrlItemStatus.Failed, cancellationToken: cancellationToken);
        var updatedFailedCount = await _urlRequestRepository.GetRequestsCountAsync(timeSpan, UrlItemStatus.Failed, UrlItemType.Updated, cancellationToken);
        var deletedFailedCount = await _urlRequestRepository.GetRequestsCountAsync(timeSpan, UrlItemStatus.Failed, UrlItemType.Deleted, cancellationToken);

        return new PeriodStatsDto
        {
            From = from,
            To = to,
            CompletedRequests = new UrlRequestStatsDto
            {
                Total = completedCount,
                Updated = updatedCompletedCount,
                Deleted = deletedCompletedCount,
                Rejected = 0
            },
            FailedRequests = new UrlRequestStatsDto
            {
                Total = failedCount,
                Updated = updatedFailedCount,
                Deleted = deletedFailedCount,
                Rejected = 0
            }
        };
    }

    public async Task<QuotaUsageDto> GetQuotaUsageAsync(DateTime? date = null, CancellationToken cancellationToken = default)
    {
        var queryDate = date ?? DateTime.UtcNow.Date;
        var timeSpan = DateTime.UtcNow - queryDate;

        var totalQuota = await _serviceAccountRepository.GetQuotaByAllAsync(cancellationToken);
        var usedQuota = await _urlRequestRepository.GetRequestsCountAsync(timeSpan, UrlItemStatus.Completed, cancellationToken: cancellationToken);

        return new QuotaUsageDto
        {
            Date = queryDate,
            TotalQuota = totalQuota,
            UsedQuota = usedQuota,
            AvailableQuota = Math.Max(0, totalQuota - usedQuota)
        };
    }
}
