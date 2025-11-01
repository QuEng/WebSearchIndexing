using Microsoft.AspNetCore.Components;
using WebSearchIndexing.Domain.Entities;
using WebSearchIndexing.Domain.Repositories;
using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Pages;

public partial class HomePage : ComponentBase
{
    private Setting _setting = null!;

    private bool _isEnabledService;

    private int _serviceAccountsCount;
    private int _quotaByServiceAccounts;
    private int _quotaBySettings;
    private int _quotaAvailableToday;
    private bool _isLoadingQuota = true;

    private int _pendingUrlRequestsCount;
    private int _updatedPendingUrlRequestsCount;
    private int _deletedPendingUrlRequestsCount;
    private bool _isLoadingPendingRequests = true;

    private int _completedUrlRequestsCount;
    private int _updatedCompletedUrlRequestsCount;
    private int _deletedCompletedUrlRequestsCount;
    private int _rejectedCompletedUrlRequestsCount;
    private bool _isLoadingCompletedRequests = true;

    private int _completedUrlRequestsTodayCount;
    private int _updatedCompletedUrlRequestsTodayCount;
    private int _deletedCompletedUrlRequestsTodayCount;
    private int _rejectedCompletedUrlRequestsTodayCount;
    private bool _isLoadingCompletedRequestsToday = true;

    [Inject]
    private IServiceAccountRepository? ServiceAccountRepository { get; set; }

    [Inject]
    private IUrlRequestRepository? UrlRequestRepository { get; set; }

    [Inject]
    private ISettingRepository? SettingRepository { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        await InitializeDataAsync();
    }

    private async Task InitializeDataAsync()
    {
        _setting = await SettingRepository!.GetSettingAsync();

        _isEnabledService = _setting.IsEnabled;

        StateHasChanged();

        _serviceAccountsCount = await ServiceAccountRepository!.GetCountAsync();
        _quotaByServiceAccounts = await ServiceAccountRepository!.GetQuotaByAllAsync();
        _quotaBySettings = _setting.RequestsPerDay;
        _quotaAvailableToday = await ServiceAccountRepository!.GetQuotaAvailableTodayAsync();
        _isLoadingQuota = false;

        StateHasChanged();

        _pendingUrlRequestsCount = await UrlRequestRepository!.GetRequestsCountAsync(UrlItemStatus.Pending);
        _updatedPendingUrlRequestsCount = await UrlRequestRepository!.GetRequestsCountAsync(UrlItemStatus.Pending, UrlItemType.Updated);
        _deletedPendingUrlRequestsCount = await UrlRequestRepository!.GetRequestsCountAsync(UrlItemStatus.Pending, UrlItemType.Deleted);
        _isLoadingPendingRequests = false;

        StateHasChanged();

        _completedUrlRequestsCount = await UrlRequestRepository!.GetRequestsCountAsync(UrlItemStatus.Completed);
        _updatedCompletedUrlRequestsCount = await UrlRequestRepository!.GetRequestsCountAsync(UrlItemStatus.Completed, UrlItemType.Updated);
        _deletedCompletedUrlRequestsCount = await UrlRequestRepository!.GetRequestsCountAsync(UrlItemStatus.Completed, UrlItemType.Deleted);
        _rejectedCompletedUrlRequestsCount = await UrlRequestRepository!.GetRequestsCountAsync(UrlItemStatus.Failed);
        _isLoadingCompletedRequests = false;

        StateHasChanged();

        _completedUrlRequestsTodayCount = await UrlRequestRepository!.GetRequestsCountAsync(TimeSpan.FromDays(1), UrlItemStatus.Completed);
        _updatedCompletedUrlRequestsTodayCount = await UrlRequestRepository!.GetRequestsCountAsync(TimeSpan.FromDays(1), UrlItemStatus.Completed, UrlItemType.Updated);
        _deletedCompletedUrlRequestsTodayCount = await UrlRequestRepository!.GetRequestsCountAsync(TimeSpan.FromDays(1), UrlItemStatus.Completed, UrlItemType.Deleted);
        _rejectedCompletedUrlRequestsTodayCount = await UrlRequestRepository!.GetRequestsCountAsync(TimeSpan.FromDays(1), UrlItemStatus.Failed);
        _isLoadingCompletedRequestsToday = false;

        StateHasChanged();
    }
}
