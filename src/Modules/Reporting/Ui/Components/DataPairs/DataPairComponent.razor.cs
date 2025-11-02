using Microsoft.AspNetCore.Components;
using ComponentBase = WebSearchIndexing.BuildingBlocks.Web.Components.ComponentBase;

namespace WebSearchIndexing.Modules.Reporting.Ui.Components.DataPairs;

public partial class DataPairComponent<T> : ComponentBase
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = null!;

    [Parameter, EditorRequired]
    public T Value { get; set; } = default!;

    [Parameter]
    public bool IsLoading { get; set; }
}
