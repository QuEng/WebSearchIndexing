using System.Diagnostics;

namespace WebSearchIndexing.Hosts.WasmHost.Theming;

public enum ThemeColor
{
    Black,
    White,

    Gray,
    GrayLight,
    GrayDarker,
    GrayDark,

    Primary,
    PrimaryRgb,
    PrimaryLighten,
    PrimaryDarken,
    PrimaryHover,

    Secondary,
    SecondaryLighten,
    SecondaryDarken,
    SecondaryHover,

    Tertiary,
    TertiaryLighten,
    TertiaryDarken,
    TertiaryHover,

    Info,
    InfoLighten,
    InfoDarken,
    InfoHover,

    Success,
    SuccessLighten,
    SuccessDarken,
    SuccessHover,

    Warning,
    WarningLighten,
    WarningDarken,
    WarningHover,

    Error,
    ErrorLighten,
    ErrorDarken,
    ErrorHover,

    Dark,
    DarkLighten,
    DarkDarken,
    DarkHover,

    TextPrimary,
    TextSecondary,
    TextDisabled,

    Background,
    SectionBackground,
    BackgroundGray
}

public static class ThemeColorExtensions
{
    private static readonly IReadOnlyDictionary<ThemeColor, string> CssTextByTheme = CreateTextMapping();
    private static readonly IReadOnlyDictionary<ThemeColor, string> ForegroundColorClassByTheme = CreateCssClassColorMapping("fg");
    private static readonly IReadOnlyDictionary<ThemeColor, string> BackgroundColorClassByTheme = CreateCssClassColorMapping("bg");

    public static string Css(this ThemeColor source) => CssTextByTheme[source];
    public static string FgColorClass(this ThemeColor source) => ForegroundColorClassByTheme[source];
    public static string BgColorClass(this ThemeColor source) => BackgroundColorClassByTheme[source];

    private static IReadOnlyDictionary<ThemeColor, string> CreateTextMapping()
    {
        var map = new Dictionary<ThemeColor, string>
        {
            { ThemeColor.Black, "--mud-palette-black"},
            { ThemeColor.White, "--mud-palette-white"},

            { ThemeColor.Gray, "--mud-palette-grey-default"},
            { ThemeColor.GrayLight, "--mud-palette-grey-light"},
            { ThemeColor.GrayDarker, "--mud-palette-grey-darker"},
            { ThemeColor.GrayDark, "--mud-palette-grey-dark"},

            { ThemeColor.Primary, "--mud-palette-primary"},
            { ThemeColor.PrimaryRgb, "--mud-palette-primary-rgb"},
            { ThemeColor.PrimaryLighten, "--mud-palette-primary-lighten"},
            { ThemeColor.PrimaryDarken, "--mud-palette-primary-darken"},
            { ThemeColor.PrimaryHover, "--mud-palette-primary-hover"},

            { ThemeColor.Secondary, "--mud-palette-secondary"},
            { ThemeColor.SecondaryLighten, "--mud-palette-secondary-lighten"},
            { ThemeColor.SecondaryDarken, "--mud-palette-secondary-darken"},
            { ThemeColor.SecondaryHover, "--mud-palette-secondary-hover"},

            { ThemeColor.Tertiary, "--mud-palette-tertiary"},
            { ThemeColor.TertiaryLighten, "--mud-palette-tertiary-lighten"},
            { ThemeColor.TertiaryDarken, "--mud-palette-tertiary-darken"},
            { ThemeColor.TertiaryHover, "--mud-palette-tertiary-hover"},

            { ThemeColor.Info, "--mud-palette-info"},
            { ThemeColor.InfoLighten, "--mud-palette-info-lighten"},
            { ThemeColor.InfoDarken, "--mud-palette-info-darken"},
            { ThemeColor.InfoHover, "--mud-palette-info-hover"},

            { ThemeColor.Success, "--mud-palette-success"},
            { ThemeColor.SuccessLighten, "--mud-palette-success-lighten"},
            { ThemeColor.SuccessDarken, "--mud-palette-success-darken"},
            { ThemeColor.SuccessHover, "--mud-palette-success-hover"},

            { ThemeColor.Warning, "--mud-palette-warning"},
            { ThemeColor.WarningLighten, "--mud-palette-warning-lighten"},
            { ThemeColor.WarningDarken, "--mud-palette-warning-darken"},
            { ThemeColor.WarningHover, "--mud-palette-warning-hover"},

            { ThemeColor.Error, "--mud-palette-error"},
            { ThemeColor.ErrorLighten, "--mud-palette-error-lighten"},
            { ThemeColor.ErrorDarken, "--mud-palette-error-darken"},
            { ThemeColor.ErrorHover, "--mud-palette-error-hover"},

            { ThemeColor.Dark, "--mud-palette-dark"},
            { ThemeColor.DarkLighten, "--mud-palette-dark-lighten"},
            { ThemeColor.DarkDarken, "--mud-palette-dark-darken"},
            { ThemeColor.DarkHover, "--mud-palette-dark-hover"},

            { ThemeColor.TextPrimary, "--mud-palette-text-primary" },
            { ThemeColor.TextSecondary, "--mud-palette-text-secondary" },
            { ThemeColor.TextDisabled, "--mud-palette-text-disabled" },

            { ThemeColor.Background, "--mud-palette-background"},
            { ThemeColor.SectionBackground, "--custom-palette-section-background" },
            { ThemeColor.BackgroundGray, "--mud-palette-background-grey"}
        };

        Debug.Assert(map.Count == Enum.GetNames(typeof(ThemeColor)).Length);

        return map;
    }

    private static IReadOnlyDictionary<ThemeColor, string> CreateCssClassColorMapping(string prefix)
    {
        var map = new Dictionary<ThemeColor, string>
        {
            { ThemeColor.Black, $"{prefix}-color-black"},
            { ThemeColor.White, $"{prefix}-color-white"},

            { ThemeColor.Gray, $"{prefix}-color-gray"},
            { ThemeColor.GrayLight, $"{prefix}-color-gray-light"},
            { ThemeColor.GrayDarker, $"{prefix}-color-gray-darker"},
            { ThemeColor.GrayDark, $"{prefix}-color-gray-dark"},

            { ThemeColor.Primary, $"{prefix}-color-primary"},
            { ThemeColor.PrimaryRgb, $"{prefix}-color-primary-rgb"},
            { ThemeColor.PrimaryLighten, $"{prefix}-color-primary-lighten"},
            { ThemeColor.PrimaryDarken, $"{prefix}-color-primary-darken"},
            { ThemeColor.PrimaryHover, $"{prefix}-color-primary-hover"},

            { ThemeColor.Secondary, $"{prefix}-color-secondary"},
            { ThemeColor.SecondaryLighten, $"{prefix}-color-secondary-lighten"},
            { ThemeColor.SecondaryDarken, $"{prefix}-color-secondary-darken"},
            { ThemeColor.SecondaryHover, $"{prefix}-color-secondary-hover"},

            { ThemeColor.Tertiary, $"{prefix}-color-tertiary"},
            { ThemeColor.TertiaryLighten, $"{prefix}-color-tertiary-lighten"},
            { ThemeColor.TertiaryDarken, $"{prefix}-color-tertiary-darken"},
            { ThemeColor.TertiaryHover, $"{prefix}-color-tertiary-hover"},

            { ThemeColor.Info, $"{prefix}-color-info"},
            { ThemeColor.InfoLighten, $"{prefix}-color-info-lighten"},
            { ThemeColor.InfoDarken, $"{prefix}-color-info-darken"},
            { ThemeColor.InfoHover, $"{prefix}-color-info-hover"},

            { ThemeColor.Success, $"{prefix}-color-success"},
            { ThemeColor.SuccessLighten, $"{prefix}-color-success-lighten"},
            { ThemeColor.SuccessDarken, $"{prefix}-color-success-darken"},
            { ThemeColor.SuccessHover, $"{prefix}-color-success-hover"},

            { ThemeColor.Warning, $"{prefix}-color-warning"},
            { ThemeColor.WarningLighten, $"{prefix}-color-warning-lighten"},
            { ThemeColor.WarningDarken, $"{prefix}-color-warning-darken"},
            { ThemeColor.WarningHover, $"{prefix}-color-warning-hover"},

            { ThemeColor.Error, $"{prefix}-color-error"},
            { ThemeColor.ErrorLighten, $"{prefix}-color-error-lighten"},
            { ThemeColor.ErrorDarken, $"{prefix}-color-error-darken"},
            { ThemeColor.ErrorHover, $"{prefix}-color-error-hover"},

            { ThemeColor.Dark, $"{prefix}-color-dark"},
            { ThemeColor.DarkLighten, $"{prefix}-color-dark-lighten"},
            { ThemeColor.DarkDarken, $"{prefix}-color-dark-darken"},
            { ThemeColor.DarkHover, $"{prefix}-color-dark-hover"},

            { ThemeColor.TextPrimary, $"{prefix}-color-text-primary" },
            { ThemeColor.TextSecondary, $"{prefix}-color-text-secondary" },
            { ThemeColor.TextDisabled, $"{prefix}-color-text-disabled" },

            { ThemeColor.Background, $"{prefix}-color-background"},
            { ThemeColor.SectionBackground, $"{prefix}-color-section-background" },
            { ThemeColor.BackgroundGray, $"{prefix}-color-background-gray"}
        };

        Debug.Assert(map.Count == Enum.GetNames(typeof(ThemeColor)).Length);

        return map;
    }
}
