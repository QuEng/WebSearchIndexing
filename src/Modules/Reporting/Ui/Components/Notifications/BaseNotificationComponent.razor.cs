using System.Collections.Frozen;
using Microsoft.AspNetCore.Components;
using ComponentBase = WebSearchIndexing.BuildingBlocks.Web.Components.ComponentBase;

namespace WebSearchIndexing.Modules.Reporting.Ui.Components.Notifications;

public partial class BaseNotificationComponent : ComponentBase
{
    private static readonly FrozenDictionary<NotificationType, string> BackgroundClassByType = new Dictionary<NotificationType, string>
    {
        { NotificationType.Success, "bg-color-success-darken" },
        { NotificationType.Info, "bg-color-info-darken" },
        { NotificationType.Warning, "bg-color-warning-darken" },
        { NotificationType.Error, "bg-color-error" },
        { NotificationType.Primary, "bg-color-primary-lighten" }
    }.ToFrozenDictionary();

    private bool _isClosed;

    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;

    [Parameter]
    public NotificationType Type { get; set; } = NotificationType.Primary;

    [Parameter]
    public EventCallback Closed { get; set; }

    private async Task CloseBannerAsync()
    {
        _isClosed = true;
        await Closed.InvokeAsync();
    }

    private string BgColorClass() => BackgroundClassByType.GetValueOrDefault(Type, "bg-color-primary-lighten");
}
