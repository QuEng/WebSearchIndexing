using MudBlazor;

namespace WebSearchIndexing.Theming;

internal class GlobalTheme : MudTheme
{
    internal GlobalTheme()
    {
        LayoutProperties = new LayoutPropertiesCustom();
        PaletteLight = new PaletteLightCustom();
        PaletteDark = new PaletteDarkCustom();
        Typography = new TypographyCustom();
        ZIndex = new ZIndex() { };
    }
}