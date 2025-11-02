# Design Theme

Decision summary
- Chosen design system: Emerald Navy (inspired by Tailwind Slate neutrals + Emerald brand hue)
- Goals met: unified brand primary, consistent Light/Dark palettes, WCAG AA contrast, full MudBlazor palette coverage, minimal custom tokens (kept `SectionBackground`).

Short research (options)
1) Slate neutrals + Indigo brand
 - Pros: clear corporate look, good AA contrast, rich shades
 - Cons: Indigo can feel colder; less aligned with current green accent in repo
2) Slate neutrals + Emerald/Teal brand (Chosen)
 - Pros: matches current green accent, positive energetic brand, great AA contrast, ample shades for hover/lighten/darken
 - Cons: needs careful separation from Success green
3) Stone neutrals + Orange brand
 - Pros: warm and friendly, distinct from Success green
 - Cons: orange on dark needs higher saturation for AA; not aligned with existing accents

Chosen mapping
- Brand Primary family: Emerald
- Secondary: Indigo
- Tertiary: Violet
- Feedback: Info (Sky), Success (Green), Warning (Amber), Error (Rose)
- Neutrals: Slate scale

Mapping to MudBlazor palette fields
- Primary: Emerald600 (Light), Emerald500 (Dark)
 - Lighten:500, Darken:700, Hover:700 (Light)
 - Lighten:400, Darken:600/700, Hover:600 (Dark)
- Secondary: Indigo600; Lighten500; Darken700; Hover700
- Tertiary: Violet600; Lighten500; Darken700; Hover700
- Info: Sky600; Lighten500; Darken700; Hover700
- Success: Green600; Lighten500; Darken700; Hover700
- Warning: Amber600; Lighten500; Darken700; Hover700
- Error: Rose600; Lighten500; Darken700; Hover700
- Neutrals (Light):
 - Background: Slate50 (#f8fafc)
 - BackgroundGrey: Slate100 (#f1f5f9)
 - Divider: Slate200 (#e2e8f0)
 - TextPrimary: Slate900 (#0f172a)
 - TextSecondary: Slate700 (#334155)
 - TextDisabled: Slate400 (#94a3b8)
 - GrayDefault: Slate300 (#cbd5e1)
 - GrayLight: Slate200 (#e2e8f0)
 - GrayDarker: Slate500 (#64748b)
 - GrayDark: Slate600 (#475569)
- Neutrals (Dark):
 - Background: Slate900 (#0f172a)
 - BackgroundGrey: between800–900 (#0b1220)
 - Divider: Slate700 (#334155)
 - TextPrimary: Slate200 (#e2e8f0)
 - TextSecondary: Slate400 (#94a3b8)
 - TextDisabled: Slate500–600 (#64748b)
 - GrayDefault: Slate600 (#475569)
 - GrayLight: Slate700 (#334155)
 - GrayDarker: Slate400 (#94a3b8)
 - GrayDark: Slate800 (#1f2937)

Surfaces
- Appbar (Light): Dark navy (#0f172a), text white
- Drawer (Light): White background; dark text; primary icons
- Appbar (Dark): Deep navy (#0b1220), text white
- Drawer (Dark): Slate800–850 background; white text/icons
- Custom token `SectionBackground`:
 - Light: White (#ffffff)
 - Dark: Slate800 (#1e293b)

WCAG notes
- Primary/Secondary/Tertiary selected to keep ?4.5:1 on text contrasts for typical usages and default contrast text color (white/near-white on filled buttons). Feedback colors use600/700 bases to ensure AA on solid fills.

No new custom tokens were introduced; `SectionBackground` retained and emitted via `CustomThemeProvider`.
