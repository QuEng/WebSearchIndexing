using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebSearchIndexing.Theming;

namespace WebSearchIndexing.Pages.Layout;

public partial class MainLayout : LayoutComponentBase
{
    private readonly string STORAGE_ACCESS_KEY = "accessKey";
    private bool _drawerOpen = false;
    private bool _isDarkMode;
    private bool _isCanShowContent = false;
    private bool _hasAccess = false;

    private CustomThemeProvider _mudThemeProvider;
    private MudTheme _customTheme = new GlobalTheme();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isDarkMode = await _mudThemeProvider.GetSystemPreference();
            _isCanShowContent = true;
            StateHasChanged();
            await _mudThemeProvider.WatchSystemPreference(OnSystemPreferenceChangedAsync);
        }
    }

    private void AllowAccess()
    {
        _hasAccess = true;
        StateHasChanged();
    }

    private async Task OnSystemPreferenceChangedAsync(bool newValue)
    {
        _isDarkMode = newValue;
        StateHasChanged();
    }

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }
}