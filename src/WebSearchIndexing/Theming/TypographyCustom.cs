using MudBlazor;

namespace WebSearchIndexing.Theming;

internal class TypographyCustom : Typography
{
    private static readonly string[] DefaultFontFamily =
    {
        "Montserrat",
        "system-ui",
        "-apple-system",
        "Segoe UI","Roboto",
        "Helvetica Neue",
        "Noto Sans",
        "Liberation Sans",
        "Arial,sans-serif",
        "Apple Color Emoji",
        "Segoe UI Emoji",
        "Segoe UI Symbol",
        "Noto Color Emoji"
    };

    internal TypographyCustom()
    {
        Default = new DefaultTypography()
        {
            FontFamily = DefaultFontFamily
        };
        H1 = new H1Typography()
        {
            FontFamily = DefaultFontFamily
        };
        H2 = new H2Typography()
        {
            FontFamily = DefaultFontFamily
        };
        H3 = new H3Typography()
        {
            FontFamily = DefaultFontFamily
        };
        H4 = new H4Typography()
        {
            FontFamily = DefaultFontFamily
        };
        H5 = new H5Typography()
        {
            FontFamily = DefaultFontFamily
        };
        H6 = new H6Typography()
        {
            FontFamily = DefaultFontFamily
        };
        Body1 = new Body1Typography()
        {
            FontFamily = DefaultFontFamily
        };
        Body2 = new Body2Typography()
        {
            FontFamily = DefaultFontFamily
        };
        Button = new ButtonTypography()
        {
            FontFamily = DefaultFontFamily,
            TextTransform = "none"
        };
        Caption = new CaptionTypography()
        {
            FontFamily = DefaultFontFamily
        };
        Overline = new OverlineTypography()
        {
            FontFamily = DefaultFontFamily
        };
        Subtitle1 = new Subtitle1Typography()
        {
            FontFamily = DefaultFontFamily
        };
        Subtitle2 = new Subtitle2Typography()
        {
            FontFamily = DefaultFontFamily
        };
    }
}