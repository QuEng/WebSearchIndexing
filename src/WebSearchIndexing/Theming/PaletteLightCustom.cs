using MudBlazor.Utilities;
using MudBlazor;

namespace WebSearchIndexing.Theming;

internal class PaletteLightCustom : PaletteLight, IPaletteCustom
{
    private const string _primary = "#111828";

    public override MudColor Black => "#0A0818";
    public override MudColor White => "#FFFFFF";
    public override MudColor Dark => "#2E2C34";

    public override MudColor Primary => _primary;

    public override MudColor TextPrimary => "#2E2C34";
    public override MudColor TextSecondary => _primary;

    public override MudColor ActionDefault => _primary;

    public override MudColor AppbarBackground => "#111828";
    public override MudColor AppbarText => White;

    public override MudColor DrawerBackground => White;
    public override MudColor DrawerText => Black;
    public override MudColor DrawerIcon => _primary;

    public override MudColor Background => "#f9fafb";
    public override MudColor BackgroundGray => "#F5F5F5";

    public override MudColor Divider => "#E0DCEA";

    public MudColor SectionBackground { get; set; } = "#fff";

    internal PaletteLightCustom()
    {
        HoverOpacity = 0.1;

        PrimaryLighten = "#363e4e";
        PrimaryDarken = "#000";

        GrayDefault = "#E0DCEA";
        GrayLight = "#EFEDF4";
        GrayDarker = "#B5B0C2";
        GrayDark = "#7A7585";
    }
}