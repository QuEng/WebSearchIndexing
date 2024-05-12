using Microsoft.AspNetCore.Components;

namespace WebSearchIndexing.Pages.Components;

/// Base for all custom components.
public abstract class ComponentBase : Microsoft.AspNetCore.Components.ComponentBase
{
    /// User class names, separated by space.
    [Parameter]
    public string? Class { get; set; }

    /// User styles, applied on top of the component's own classes and styles.
    [Parameter]
    public string? Style { get; set; }
}