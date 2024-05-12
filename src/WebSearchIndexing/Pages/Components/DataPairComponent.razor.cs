using Microsoft.AspNetCore.Components;

namespace WebSearchIndexing.Pages.Components;

public partial class DataPairComponent<T> : ComponentBase
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = null!;

    [Parameter, EditorRequired]
    public T Value { get; set; } = default!;

    [Parameter]
    public bool IsLoading { get; set; }
}