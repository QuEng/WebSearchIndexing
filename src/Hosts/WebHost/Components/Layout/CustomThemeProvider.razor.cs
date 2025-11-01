using System.Text;
using MudBlazor;
using WebSearchIndexing.Hosts.WebHost.Theming;

namespace WebSearchIndexing.Hosts.WebHost.Components.Layout;

public partial class CustomThemeProvider : MudThemingProvider
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

        if ((IsDarkMode ? Theme.PaletteDark : Theme.Palette) is not IPaletteCustom palette)
        {
            return;
        }

        theme.AppendLine($"{ThemeColor.SectionBackground.Css()}: {palette.SectionBackground};");

        base.GenerateTheme(theme);
    }
}
