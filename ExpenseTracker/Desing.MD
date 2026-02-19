# Expense Tracker — Design Reference

## Screens
| Name | Frame ID | Size |
|------|----------|------|
| Expense Tracker - Home | `RONup` | 433 × 1673 |
| Add Card | `5uAhM` | 402 × 874 |

## Themes
Mode axis: `Light` / `Dark`. Current screens use **Dark Mode**.

---

## Design Tokens (Variables)

### Typography
| Token | Value |
|-------|-------|
| `--font-primary` | `JetBrains Mono` |
| `--font-secondary` | `Geist` |
> Screens also use **Space Grotesk** (hardcoded) for all UI labels.

### Border Radius
| Token | Value |
|-------|-------|
| `--radius-none` | `0` |
| `--radius-m` | `16` |
| `--radius-pill` | `999` |
> Also observed: `26` (tab items), `36` (tab pill container)

### Core Colors (Light → Dark)
| Token | Light | Dark |
|-------|-------|------|
| `--background` | `#F2F3F0` | `#111111` |
| `--foreground` | `#111111` | `#FFFFFF` |
| `--card` | `#FFFFFF` | `#1A1A1A` |
| `--card-foreground` | `#111111` | `#FFFFFF` |
| `--primary` | `#FF8400` | `#FF8400` |
| `--primary-foreground` | `#111111` | `#111111` |
| `--secondary` | `#E7E8E5` | `#2E2E2E` |
| `--secondary-foreground` | `#111111` | `#FFFFFF` |
| `--muted` | `#F2F3F0` | `#2E2E2E` |
| `--muted-foreground` | `#666666` | `#B8B9B6` |
| `--destructive` | `#D93C15` | `#FF5C33` |
| `--border` | `#CBCCC9` | `#2E2E2E` |
| `--input` | `#CBCCC9` | `#2E2E2E` |

### Status Colors
| Token | Light | Dark |
|-------|-------|------|
| `--color-error-foreground` | `#8C1C00` | `#FF5C33` |
| `--color-success-foreground` | `#004D1A` | `#B6FFCE` |
| `--color-warning-foreground` | `#804200` | `#FF8400` |
| `--color-info-foreground` | `#000066` | `#B2B2FF` |

---

## Hardcoded Dark-Mode Palette
| Role | Hex |
|------|-----|
| App background | `#111111` |
| Card surface | `#1C1C1C` |
| Elevated surface | `#282828` |
| Stroke / border | `#2A2A2A` |
| Accent gold | `#D4AF37` |
| Accent gold 10% | `#D4AF3720` |
| Success green | `#4CAF50` |
| Success green 10% | `#4CAF5020` |
| Error red | `#F44336` |
| Muted text | `#666666` |
| Dimmed text | `#A0A0A0` |
| White text | `#FFFFFF` |
| Transparent (ghost btn) | `#00000000` |

---

## Typography Scale
| px | Usage |
|----|-------|
| 9 | Tab labels |
| 10 | Micro / uppercase captions |
| 12–13 | Footnotes, captions |
| 14–15 | Body |
| 18 | Sub-heading |
| 22–24 | Section heading |
| 28 | Large heading |
| 36 | Display / hero amount |
| 42 | Large balance display |

**Weights:** 400, 500, 600, 700  
**Letter spacing:** `0.5` (tab labels), `2` (uppercase captions)

---

## Spacing

**Gap scale:** `0 2 4 5 6 8 10 12 14 16 20 24 32`

**Key paddings (T R B L):**
- Screen content wrap: `8 28 28 28`
- Scroll area (nav offset): `0 0 90 0`
- Card / row: `18 16 18 16`
- Summary card: `20 24 20 24`
- Tab bar container: `12 21 21 21`
- Form section: `0 28 40 28`

---

## Component Anatomy

**Cards:** `#1C1C1C` + `1px inside #2A2A2A` + `radius 16` + padding `16–18`  
**Left-accent cards:** replace full border with `3px left stroke #D4AF37`  
**Primary CTA:** `#D4AF37` fill, `#111111` text, height `56`, full width  
**Ghost button:** transparent, `1px #2A2A2A` border, height `48`  
**Tab bar:** `#1C1C1C` container `radius 36`, active pill `#D4AF37 radius 26`  
**Avatar circle:** `44×44`, `#D4AF37`, `radius 999`  
**Back button:** `40×40`, `#1C1C1C`, `1px #2A2A2A`

---

## Icons
Library: **Lucide**, default size `18×18`

---

## Conventions
1. Dark-first — `#111111` bg, surfaces `#1C1C1C` / `#282828`
2. Gold `#D4AF37` = primary actions and active states only
3. No shadows — depth via fill steps + 1px borders
4. Green/Red only for financial status
5. `Space Grotesk` for all UI chrome; uppercase + letter-spacing for labels
6. `padding-bottom: 90` on scroll areas to clear floating tab bar
