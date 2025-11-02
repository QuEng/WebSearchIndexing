using Microsoft.AspNetCore.Components;
using WebSearchIndexing.Modules.Reporting.Application.DTOs;
using WebSearchIndexing.Modules.Reporting.Ui.Services;

namespace WebSearchIndexing.Modules.Reporting.Ui.Pages.Dashboard;

public partial class DashboardPage : ComponentBase
{
    private DashboardStatsDto? _dashboardStats;
    private bool _isLoading = true;
    private string? _errorMessage;

    [Inject]
    private IReportingHttpClient ReportingHttpClient { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;
            StateHasChanged();

            _dashboardStats = await ReportingHttpClient.GetDashboardStatsAsync();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load dashboard data: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    // Helper properties for backward compatibility with the existing Razor template
    private bool _isEnabledService => _dashboardStats?.IsServiceEnabled ?? false;
    
    private int _serviceAccountsCount => _dashboardStats?.ServiceAccounts.Count ?? 0;
    private int _quotaByServiceAccounts => _dashboardStats?.Quota.TotalQuotaByServiceAccounts ?? 0;
    private int _quotaBySettings => _dashboardStats?.Quota.QuotaBySettings ?? 0;
    private int _quotaAvailableToday => _dashboardStats?.Quota.AvailableToday ?? 0;
    private bool _isLoadingQuota => _isLoading;

    private int _pendingUrlRequestsCount => _dashboardStats?.PendingRequests.Total ?? 0;
    private int _updatedPendingUrlRequestsCount => _dashboardStats?.PendingRequests.Updated ?? 0;
    private int _deletedPendingUrlRequestsCount => _dashboardStats?.PendingRequests.Deleted ?? 0;
    private bool _isLoadingPendingRequests => _isLoading;

    private int _completedUrlRequestsCount => _dashboardStats?.CompletedRequests.Total ?? 0;
    private int _updatedCompletedUrlRequestsCount => _dashboardStats?.CompletedRequests.Updated ?? 0;
    private int _deletedCompletedUrlRequestsCount => _dashboardStats?.CompletedRequests.Deleted ?? 0;
    private int _rejectedCompletedUrlRequestsCount => _dashboardStats?.CompletedRequests.Rejected ?? 0;
    private bool _isLoadingCompletedRequests => _isLoading;

    private int _completedUrlRequestsTodayCount => _dashboardStats?.CompletedRequestsToday.Total ?? 0;
    private int _updatedCompletedUrlRequestsTodayCount => _dashboardStats?.CompletedRequestsToday.Updated ?? 0;
    private int _deletedCompletedUrlRequestsTodayCount => _dashboardStats?.CompletedRequestsToday.Deleted ?? 0;
    private int _rejectedCompletedUrlRequestsTodayCount => _dashboardStats?.CompletedRequestsToday.Rejected ?? 0;
    private bool _isLoadingCompletedRequestsToday => _isLoading;
}

