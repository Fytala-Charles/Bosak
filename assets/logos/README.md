inkscape
# FYTALA Brand Pack (Poppins + Fixed Palette)
This package contains cleaned SVG logos, 24×24 icons, and circular badges that use ONLY the approved palette and `Poppins` font family.

## Palette
{
  "DARK_1": "#556B2F",
  "LIGHT_1": "#F0FFF0",
  "DARK_2": "#2F4F4F",
  "LIGHT_2": "#E8F4EE",
  "ACCENT_1": "#293F5F",
  "ACCENT_2": "#50639C",
  "ACCENT_3": "#5178A8",
  "ACCENT_4": "#518D8F",
  "ACCENT_5": "#98D481",
  "ACCENT_6": "#FDF2CF",
  "HYPERLINK": "#008080",
  "FOLLOWED_HL": "#483D8B",
  "BACKGROUND": "#E8F4EE",
  "TEXT_DEFAULT": "#2F4F4F",
  "TEXT_ALT": "#556B2F",
  "HEADER_BG": "#2F4F4F",
  "HEADER_TEXT": "#F0FFF0"
}

## Contents
- `assets/images/*-clean.svg` — original logos cleaned to palette + Poppins.
- `assets/icons/*-24.svg` — 24×24 icons.
- `assets/icons/*-badge-*.svg` — 24×24 badges (circle backgrounds), incl. mono light/dark.
- `assets/css/fytala-brand.css` — CSS variables and utilities.

## Notes
- If your environment is offline, self-host the Poppins font and reference it in your main CSS.
- All SVG color values have been mapped to the nearest palette color if they were out of gamut.
- Avoid editing these with tools that reintroduce CSS variables; keep exports flattened to hex.
