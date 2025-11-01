using MudBlazor;
using WebSearchIndexing.BuildingBlocks.Web.Navigation;
using WebSearchIndexing.Hosts.WebHost.Theming;

namespace WebSearchIndexing.Hosts.WebHost.Components.Layout;

public partial class MainLayout : LayoutComponentBase
{
    private readonly List<NavigationItem> _navigation = [];
    private bool _drawerOpen;
    private bool _isDarkMode;
    private bool _isReady;
    private bool _hasAccess;
    private CustomThemeProvider? _themeProvider;
    private MudTheme _customTheme = new GlobalTheme();

    [Inject]
    private INavigationService NavigationService { get; set; } = default!;

    protected override void OnInitialized()
    {
        _navigation.AddRange(NavigationService.GetItems());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        if (_themeProvider is not null)
        {
            _isDarkMode = await _themeProvider.GetSystemPreference();
            await _themeProvider.WatchSystemPreference(OnSystemPreferenceChangedAsync);
        }

        _isReady = true;
        StateHasChanged();
    }

    private Task OnSystemPreferenceChangedAsync(bool newValue)
    {
        _isDarkMode = newValue;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private void AllowAccess()
    {
        _hasAccess = true;
        StateHasChanged();
    }

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }
}
