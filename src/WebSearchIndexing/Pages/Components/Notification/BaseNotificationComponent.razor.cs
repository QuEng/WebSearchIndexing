using Microsoft.AspNetCore.Components;
using WebSearchIndexing.Theming;

namespace WebSearchIndexing.Pages.Components;

public partial class BaseNotificationComponent : ComponentBase
{
    private bool _isClosed;

    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;

    [Parameter]
    public NotificationType Type { get; set; }

    [Parameter]
    public EventCallback Closed { get; set; }

    private async Task CloseBannerAsync()
    {
        _isClosed = true;
        await Closed.InvokeAsync();
    }

    private string BgColorClass()
        => Type switch
        {
            NotificationType.Success => ThemeColor.SuccessDarken.BgColorClass(),
            NotificationType.Info => ThemeColor.InfoDarken.BgColorClass(),
            NotificationType.Warning => ThemeColor.WarningDarken.BgColorClass(),
            NotificationType.Error => ThemeColor.Error.BgColorClass(),
            NotificationType.Primary => ThemeColor.PrimaryLighten.BgColorClass(),
            _ => ThemeColor.PrimaryLighten.BgColorClass()
        };
}
