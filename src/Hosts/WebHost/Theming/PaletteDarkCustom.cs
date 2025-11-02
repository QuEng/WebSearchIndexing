using MudBlazor;
using MudBlazor.Utilities;

namespace WebSearchIndexing.Hosts.WebHost.Theming;

internal class PaletteDarkCustom : PaletteDark, IPaletteCustom
{
    // Brand Primary: Emerald
    private const string _primary = "#10B981"; // emerald-500 (brighter on dark)
    public override MudColor Primary => _primary;

    public override MudColor Black => "#000000";
    public override MudColor White => "#FFFFFF";

    // Backgrounds & surfaces
    public override MudColor Background => "#0F172A"; // slate-900
    public override MudColor BackgroundGrey => "#0B1220"; // deep navy gray
    public override MudColor Divider => "#334155"; // slate-700

    public override MudColor AppbarBackground => "#0B1220"; // deep navy
    public override MudColor AppbarText => White;
    public override MudColor DrawerBackground => "#1E293B"; // slate-800
    public override MudColor DrawerText => White;
    public override MudColor DrawerIcon => White;

    // Text
    public override MudColor TextPrimary => "#E2E8F0"; // slate-200
    public override MudColor TextSecondary => "#94A3B8"; // slate-400
    public override MudColor TextDisabled => "#64748B"; // slate-500/600

    public MudColor SectionBackground { get; set; } = "#1E293B"; // slate-800 surface

    internal PaletteDarkCustom()
    {
        // Interactions
        HoverOpacity = 0.1;

        // Primary shades for dark
        PrimaryLighten = "#34D399"; // emerald-400
        PrimaryDarken = "#059669"; // emerald-600

        // Secondary: Indigo
        Secondary = "#6366F1"; // indigo-500 (better pop on dark)
        SecondaryLighten = "#818CF8"; // indigo-400
        SecondaryDarken = "#4F46E5"; // indigo-600

        // Tertiary: Violet
        Tertiary = "#8B5CF6"; // violet-500
        TertiaryLighten = "#A78BFA"; // violet-400
        TertiaryDarken = "#7C3AED"; // violet-600

        // Info: Sky
        Info = "#0EA5E9"; // sky-500
        InfoLighten = "#38BDF8"; // sky-400
        InfoDarken = "#0284C7"; // sky-600

        // Success: Green
        Success = "#22C55E"; // green-500
        SuccessLighten = "#4ADE80"; // green-400
        SuccessDarken = "#16A34A"; // green-600

        // Warning: Amber
        Warning = "#F59E0B"; // amber-500
        WarningLighten = "#FBBF24"; // amber-400
        WarningDarken = "#D97706"; // amber-600

        // Error: Rose
        Error = "#F43F5E"; // rose-500
        ErrorLighten = "#FB7185"; // rose-400
        ErrorDarken = "#E11D48"; // rose-600

        // Gray scale
        GrayDefault = "#475569"; // slate-600
        GrayLight = "#334155"; // slate-700
        GrayDarker = "#94A3B8"; // slate-400
        GrayDark = "#1F2937"; // between slate-800 and gray-800 tone

        // Dark tonal ramps used by utilities
        DarkLighten = "#1E293B";
        DarkDarken = "#0A0F1C";
    }
}
