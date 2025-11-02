using MudBlazor.Utilities;
using MudBlazor;

namespace WebSearchIndexing.Hosts.WebHost.Theming;

internal class PaletteLightCustom : PaletteLight, IPaletteCustom
{
    // Brand Primary: Emerald
    private const string _primary = "#059669"; // emerald-600

    public override MudColor Black => "#0A0818";
    public override MudColor White => "#FFFFFF";

    // Neutral dark swatch used by some components
    public override MudColor Dark => "#1E293B"; // slate-800

    // Brand
    public override MudColor Primary => _primary;

    // Text
    public override MudColor TextPrimary => "#0F172A"; // slate-900
    public override MudColor TextSecondary => "#334155"; // slate-700
    public override MudColor TextDisabled => "#94A3B8"; // slate-400

    // Buttons and actions
    public override MudColor ActionDefault => _primary;

    // Surfaces
    public override MudColor AppbarBackground => "#0F172A"; // deep navy
    public override MudColor AppbarText => White;

    public override MudColor DrawerBackground => White;
    public override MudColor DrawerText => Black;
    public override MudColor DrawerIcon => _primary;

    public override MudColor Background => "#F8FAFC"; // slate-50
    public override MudColor BackgroundGrey => "#F1F5F9"; // slate-100

    public override MudColor Divider => "#E2E8F0"; // slate-200

    public MudColor SectionBackground { get; set; } = "#FFFFFF"; // custom token

    internal PaletteLightCustom()
    {
        // Interactions
        HoverOpacity =0.1;

        // Primary shades
        PrimaryLighten = "#10B981"; // emerald-500
        PrimaryDarken = "#047857"; // emerald-700

        // Secondary: Indigo
        Secondary = "#4F46E5"; // indigo-600
        SecondaryLighten = "#6366F1"; // indigo-500
        SecondaryDarken = "#4338CA"; // indigo-700

        // Tertiary: Violet
        Tertiary = "#7C3AED"; // violet-600
        TertiaryLighten = "#8B5CF6"; // violet-500
        TertiaryDarken = "#6D28D9"; // violet-700

        // Info: Sky
        Info = "#0284C7"; // sky-600
        InfoLighten = "#0EA5E9"; // sky-500
        InfoDarken = "#0369A1"; // sky-700

        // Success: Green (distinct from emerald)
        Success = "#16A34A"; // green-600
        SuccessLighten = "#22C55E"; // green-500
        SuccessDarken = "#15803D"; // green-700

        // Warning: Amber
        Warning = "#D97706"; // amber-600
        WarningLighten = "#F59E0B"; // amber-500
        WarningDarken = "#B45309"; // amber-700

        // Error: Rose
        Error = "#E11D48"; // rose-600
        ErrorLighten = "#F43F5E"; // rose-500
        ErrorDarken = "#BE123C"; // rose-700

        // Gray scale
        GrayDefault = "#CBD5E1"; // slate-300
        GrayLight = "#E2E8F0"; // slate-200
        GrayDarker = "#64748B"; // slate-500
        GrayDark = "#475569"; // slate-600

        // Dark tonal ramps for utilities
        DarkLighten = "#334155"; // slate-700
        DarkDarken = "#0B1220"; // deep navy variant
    }
}
