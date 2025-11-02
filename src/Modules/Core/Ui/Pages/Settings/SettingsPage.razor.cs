using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebSearchIndexing.Modules.Core.Application.BackgroundJobs;
using WebSearchIndexing.Modules.Core.Ui.Services;
using WebSearchIndexing.Modules.Core.Application.DTOs;

namespace WebSearchIndexing.Modules.Core.Ui.Pages.Settings;

public partial class SettingsPage : ComponentBase
{
    private SettingsDto _setting = new(Guid.Empty, Guid.Empty, "", 0, false);
    private int _currentRequestsPerDay;
    private int _oldValueRequestsPerDay;
    private int _maxRequestsPerDay;
    private bool _isLoadingSettings = true;
    private bool _isSavingSettings;

    [Inject]
    private ICoreHttpClient CoreHttpClient { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private IScopedRequestSendingService RequestSendingService { get; set; } = default!;

    private bool RequestsPerDayChanged => _currentRequestsPerDay != _oldValueRequestsPerDay;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var loadedSettings = await CoreHttpClient.GetSettingsAsync();
        if (loadedSettings is null)
        {
            Snackbar!.Add("Failed to load settings", Severity.Error);
            _isLoadingSettings = false;
            StateHasChanged();
            return;
        }
        
        _setting = loadedSettings;
        _currentRequestsPerDay = _setting.RequestsPerDay;
        _oldValueRequestsPerDay = _setting.RequestsPerDay;

        var serviceAccounts = await CoreHttpClient.GetServiceAccountsAsync();
        _maxRequestsPerDay = (int)(serviceAccounts?.Sum(x => x.QuotaLimitPerDay) ?? 0);
        _isLoadingSettings = false;
        StateHasChanged();
    }

    private async Task ToggleEnablingAsync()
    {
        if (_isSavingSettings)
        {
            return;
        }

        if (_setting.IsEnabled)
        {
            var result = await DialogService!.ShowMessageBox(
                "Disabling service",
                "Are you sure you want to disable service?",
                yesText: "Disable",
                noText: "Cancel");

            if (result is null or false)
            {
                return;
            }
        }

        var newIsEnabled = !_setting.IsEnabled;
        _isSavingSettings = true;

        var updatedSettings = await CoreHttpClient.UpdateSettingsAsync(_setting.RequestsPerDay, newIsEnabled);
        if (updatedSettings is not null)
        {
            _setting = updatedSettings;
            if (_setting.IsEnabled)
            {
                _ = Task.Run(async () => await RequestSendingService.DoWork(new()));
            }

            Snackbar!.Add("Settings updated", Severity.Success);
            _isSavingSettings = false;
            StateHasChanged();
        }
        else
        {
            Snackbar!.Add("Failed to update settings", Severity.Error);
            _isSavingSettings = false;
            StateHasChanged();
        }
    }

    private async Task SaveRequestsPerDayAsync()
    {
        if (_isSavingSettings ||
            RequestsPerDayChanged is false)
        {
            return;
        }

        _isSavingSettings = true;

        var updatedSettings = await CoreHttpClient.UpdateSettingsAsync(_currentRequestsPerDay);
        if (updatedSettings is not null)
        {
            _setting = updatedSettings;
            _currentRequestsPerDay = _setting.RequestsPerDay;
            Snackbar!.Add("Settings updated", Severity.Success);
            _oldValueRequestsPerDay = _setting.RequestsPerDay;
            _isSavingSettings = false;
            StateHasChanged();
        }
        else
        {
            Snackbar!.Add("Failed to update settings", Severity.Error);
            // Reload from server to reset changes
            await LoadDataAsync();
            _isSavingSettings = false;
            StateHasChanged();
        }
    }

    private async Task LoadDataAsync()
    {
        var loadedSettings = await CoreHttpClient.GetSettingsAsync();
        if (loadedSettings is not null)
        {
            _setting = loadedSettings;
            _currentRequestsPerDay = _setting.RequestsPerDay;
            _oldValueRequestsPerDay = _setting.RequestsPerDay;
        }
    }
}
