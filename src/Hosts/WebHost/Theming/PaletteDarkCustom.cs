using MudBlazor;
using MudBlazor.Utilities;

namespace WebSearchIndexing.Hosts.WebHost.Theming;

internal class PaletteDarkCustom : PaletteDark, IPaletteCustom
{
    private const string _primary = "#02a870";
    public override MudColor Primary => _primary;

    public override MudColor Background => "#27272f";
    public override MudColor AppbarBackground => "#101828";
    public override MudColor AppbarText => White;
    public override MudColor DrawerBackground => "#373740";
    public override MudColor DrawerText => White;
    public override MudColor DrawerIcon => White;

    public MudColor SectionBackground { get; set; } = "#373740";
}
