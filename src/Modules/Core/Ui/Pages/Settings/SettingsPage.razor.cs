using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebSearchIndexing.Domain.Entities;
using WebSearchIndexing.Domain.Repositories;
using WebSearchIndexing.Modules.Core.Application.BackgroundJobs;

namespace WebSearchIndexing.Modules.Core.Ui.Pages.Settings;

public partial class SettingsPage : ComponentBase
{
    private Setting _setting = null!;
    private int _oldValueRequestsPerDay;
    private int _maxRequestsPerDay;
    private bool _isLoadingSettings = true;
    private bool _isSavingSettings;

    [Inject]
    private ISettingRepository SettingRepository { get; set; } = default!;

    [Inject]
    private IServiceAccountRepository ServiceAccountRepository { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private IScopedRequestSendingService RequestSendingService { get; set; } = default!;

    private bool RequestsPerDayChanged => _setting.RequestsPerDay != _oldValueRequestsPerDay;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _setting = await SettingRepository.GetSettingAsync();
        if (_setting is null)
        {
            Snackbar!.Add("Failed to load settings", Severity.Error);
            _isLoadingSettings = false;
            StateHasChanged();
            return;
        }

        _oldValueRequestsPerDay = _setting.RequestsPerDay;

        var serviceAccounts = await ServiceAccountRepository.GetAllAsync();
        _maxRequestsPerDay = (int)serviceAccounts.Sum(x => x.QuotaLimitPerDay);
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

        _setting.IsEnabled = !_setting.IsEnabled;
        _isSavingSettings = true;

        if (await SettingRepository.UpdateAsync(_setting))
        {
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
            _setting.IsEnabled = !_setting.IsEnabled;
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

        if (await SettingRepository.UpdateAsync(_setting))
        {
            Snackbar!.Add("Settings updated", Severity.Success);
            _oldValueRequestsPerDay = _setting.RequestsPerDay;
            _isSavingSettings = false;
            StateHasChanged();
        }
        else
        {
            Snackbar!.Add("Failed to update settings", Severity.Error);
            _setting.RequestsPerDay = _oldValueRequestsPerDay;
            _isSavingSettings = false;
            StateHasChanged();
        }
    }
}
