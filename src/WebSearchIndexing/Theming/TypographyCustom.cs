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
        Default = new Default()
        {
            FontFamily = DefaultFontFamily
        };
        H1 = new H1()
        {
            FontFamily = DefaultFontFamily
        };
        H2 = new H2()
        {
            FontFamily = DefaultFontFamily
        };
        H3 = new H3()
        {
            FontFamily = DefaultFontFamily
        };
        H4 = new H4()
        {
            FontFamily = DefaultFontFamily
        };
        H5 = new H5()
        {
            FontFamily = DefaultFontFamily
        };
        H6 = new H6()
        {
            FontFamily = DefaultFontFamily
        };
        Body1 = new Body1()
        {
            FontFamily = DefaultFontFamily
        };
        Body2 = new Body2()
        {
            FontFamily = DefaultFontFamily
        };
        Button = new Button()
        {
            FontFamily = DefaultFontFamily,
            TextTransform = "none"
        };
        Caption = new Caption()
        {
            FontFamily = DefaultFontFamily
        };
        Overline = new Overline()
        {
            FontFamily = DefaultFontFamily
        };
        Subtitle1 = new Subtitle1()
        {
            FontFamily = DefaultFontFamily
        };
        Subtitle2 = new Subtitle2()
        {
            FontFamily = DefaultFontFamily
        };
    }
}