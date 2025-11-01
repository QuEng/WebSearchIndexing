using Microsoft.AspNetCore.Components;

namespace WebSearchIndexing.BuildingBlocks.Web.Components;

/// <summary>
/// Base class for reusable Blazor components.
/// </summary>
public abstract class ComponentBase : Microsoft.AspNetCore.Components.ComponentBase
{
    /// <summary>
    /// User-defined CSS classes appended to the component.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// User-defined CSS styles appended to the component.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }
}
