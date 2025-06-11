using MudBlazor;
using System.Text;
using WebSearchIndexing.Theming;

namespace WebSearchIndexing.Pages.Layout;

public partial class CustomThemeProvider : MudThemeProvider
{
    protected new string BuildTheme()
    {
        Theme ??= new MudTheme();
        var theme = new StringBuilder();
        theme.AppendLine("<style>");
        theme.Append(Theme.PseudoCss.Scope);
        theme.AppendLine("{");
        GenerateTheme(theme);
        theme.AppendLine("}");
        theme.AppendLine("</style>");

        return theme.ToString();
    }

    protected override void GenerateTheme(StringBuilder theme)
    {
        if (Theme is null)
        {
            return;
        }

        IPaletteCustom palette = (IsDarkMode ? Theme.PaletteDark as PaletteDarkCustom : Theme.PaletteLight as PaletteLightCustom)!;

        if (palette is null)
        {
            return;
        }

        theme.AppendLine($"{ThemeColor.SectionBackground.Css()}: {palette.SectionBackground};");

        base.GenerateTheme(theme);
    }
}