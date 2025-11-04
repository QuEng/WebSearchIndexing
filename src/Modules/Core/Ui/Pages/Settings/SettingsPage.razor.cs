using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebSearchIndexing.Contracts.Catalog;
using WebSearchIndexing.Modules.Core.Application.DTOs;
using WebSearchIndexing.Modules.Core.Ui.Contracts;
using WebSearchIndexing.Modules.Core.Ui.Models;

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
    private ICoreApiService CoreApiService { get; set; } = default!;

    [Inject]
    private IServiceAccountsApiService ServiceAccountsApiService { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    private bool RequestsPerDayChanged => _currentRequestsPerDay != _oldValueRequestsPerDay;

    protected override async Task OnInitializedAsync()
    {
        await LoadInitialDataAsync();
    }

    private async Task LoadInitialDataAsync()
    {
        var loadedSettings = await CoreApiService.GetSettingsAsync();
        if (loadedSettings is null)
        {
            Snackbar.Add("Failed to load settings", Severity.Error);
            _isLoadingSettings = false;
            return;
        }

        _setting = loadedSettings;
        _currentRequestsPerDay = _setting.RequestsPerDay;
        _oldValueRequestsPerDay = _setting.RequestsPerDay;

        var serviceAccounts = await ServiceAccountsApiService.GetAllAsync();
        _maxRequestsPerDay = (int)(serviceAccounts?.Sum(x => x.QuotaLimitPerDay) ?? 0);
        _isLoadingSettings = false;
    }

    private async Task ToggleEnablingAsync()
    {
        if (_isSavingSettings)
        {
            return;
        }

        if (_setting.IsEnabled)
        {
            var result = await DialogService.ShowMessageBox(
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

        var request = new UpdateSettingsRequest(_setting.RequestsPerDay, newIsEnabled);
        var updatedSettings = await CoreApiService.UpdateSettingsAsync(request);
        if (updatedSettings is not null)
        {
            _setting = updatedSettings;
            if (_setting.IsEnabled)
            {
                // ??????? API endpoint ??? ??????? ???????? ????????? ??????
                try
                {
                    await CoreApiService.TriggerProcessingAsync();
                }
                catch
                {
                    Snackbar.Add("Settings updated, but failed to trigger processing", Severity.Warning);
                }
            }

            Snackbar.Add("Settings updated", Severity.Success);
            _isSavingSettings = false;
            // StateHasChanged() ??????????? ?????????????
        }
        else
        {
            Snackbar.Add("Failed to update settings", Severity.Error);
            _isSavingSettings = false;
            // StateHasChanged() ??????????? ?????????????
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

        var request = new UpdateSettingsRequest(_currentRequestsPerDay);
        var updatedSettings = await CoreApiService.UpdateSettingsAsync(request);
        if (updatedSettings is not null)
        {
            _setting = updatedSettings;
            _currentRequestsPerDay = _setting.RequestsPerDay;
            Snackbar.Add("Settings updated", Severity.Success);
            _oldValueRequestsPerDay = _setting.RequestsPerDay;
            _isSavingSettings = false;
            // StateHasChanged() ??????????? ?????????????
        }
        else
        {
            Snackbar.Add("Failed to update settings", Severity.Error);
            // Reload from server to reset changes
            await LoadDataAsync();
            _isSavingSettings = false;
            // StateHasChanged() ??????????? ?????????????
        }
    }

    private async Task LoadDataAsync()
    {
        var loadedSettings = await CoreApiService.GetSettingsAsync();
        if (loadedSettings is not null)
        {
            _setting = loadedSettings;
            _currentRequestsPerDay = _setting.RequestsPerDay;
            _oldValueRequestsPerDay = _setting.RequestsPerDay;
        }
    }
}
