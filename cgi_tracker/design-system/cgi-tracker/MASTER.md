# Design System Master File

> **LOGIC:** When building a specific page, first check `design-system/pages/[page-name].md`.
> If that file exists, its rules **override** this Master file.
> If not, strictly follow the rules below.

---

**Project:** CGI Tracker  
**Pattern:** Bento Grids (UI/UX Pro Max)  
**Generated:** 2026-07-19  
**Design Dials:** Variance 9/10 (Bold / Asymmetric) | Motion 4/10 (Standard) | Density 7/10

---

## Global Rules

### Layout Pattern

**Bento Grids** — modular, Apple-style cards with varied spans (1×1, 2×1, 2×2), clean hierarchy, soft surfaces.

- CSS Grid, not equal-column flex for primary layouts
- Rounded corners 16–24px
- Soft page background, white cards, subtle borders/shadows
- Hover: scale ~1.02, soft shadow expansion (respect `prefers-reduced-motion`)
- Responsive: 4 → 2 → 1 columns

### Color Palette (CGI brand + bento neutrals)

| Role | Hex | CSS Variable |
|------|-----|--------------|
| Brand / Navbar | `#2A1809` | `--color-brand` |
| Primary | `#5E3D0F` | `--color-primary` |
| On Primary | `#FFFFFF` | `--color-on-primary` |
| Secondary | `#8A4B24` | `--color-secondary` |
| Accent | `#8B6914` | `--color-accent` |
| Background | `#F5F5F7` | `--color-background` |
| Foreground | `#1D1D1F` | `--color-foreground` |
| Muted | `#F5F5F7` | `--color-muted` |
| Border | `#E5E5EA` | `--color-border` |
| Card | `#FFFFFF` | `--color-card` |
| Destructive | `#DC2626` | `--color-destructive` |
| Ring | `#5E3D0F` | `--color-ring` |

**Notes:** Keep CGI brown family for headers/nav. Use Apple-like off-white bento canvas (`#F5F5F7`), not cream/amber.

### Card header accents (ticket modules)

| Module | Hex |
|--------|-----|
| Description | `#5E3D0F` |
| Change Status | `#8A4B24` |
| Upload | `#5A6B3A` |
| Current Status | `#8B6914` |
| Allocated Hours | `#6B4A3A` |
| Client Attachments | `#4A6B5A` |
| Support Attachments | `#3D5A6C` |

### Typography

- **Font:** Plus Jakarta Sans (headings + body)
- **Mood:** friendly, modern, SaaS, professional
- **Google Fonts:** `https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@300;400;500;600;700&display=swap`

### Spacing

| Token | Value |
|-------|-------|
| `--grid-gap` | `16px` |
| `--card-radius` | `20px` |
| `--space-md` | `16px` |
| `--space-lg` | `24px` |

### Effects

- Soft shadow: `0 4px 16px rgba(42, 24, 9, 0.06)`
- Hover: `transform: scale(1.01)`; shadow deepen
- Transition: `200ms ease`
- No emoji icons; use text/SVG

### Avoid

- Equal-width side-by-side panels as the primary layout
- Cream/terracotta “AI default” looks
- Purple gradients, glow pills, heavy multi-layer shadows
- Hidden critical actions

---

## How to Apply

1. Page canvas: `--color-background`
2. Content in bento tiles (`.ticket-card` / `.bento-tile`) with varied `grid-column` spans
3. Module headers keep brand accent colors
4. Check `pages/[page].md` for overrides
