# Corporate Chaos — Customization Guide

This guide explains how to modify Corporate Chaos without writing any code. All customization is done by editing JSON and XAML files with a text editor.

---

## Quick Reference

| What You Want to Change | File to Edit |
|------------------------|-------------|
| Employee names | `data/names.json` |
| Random events and crises | `data/events.json` |
| Game balance (costs, revenue, probabilities) | `data/gamebalance.json` |
| Job descriptions and skills | `data/positions.json` |
| Button colors and styles | `styles/ButtonStyles.xaml` |
| Fonts and text sizes | `styles/Typography.xaml` |
| Scrollbar appearance | `styles/ScrollBarStyles.xaml` |
| Game theme (title bars, panels, shadows) | `styles/GameTheme.xaml` |
| Department images | `images/*.png` |
| Employee portraits | `images/emp_male/*.png`, `images/emp_female/*.png` |
| Character portraits | `images/char/*.png` |
| Background music | `audio/background.mp3` |

---

## 1. Editing Employee Names

**File:** `data/names.json`

This file contains three arrays: male first names, female first names, and last names. The game randomly combines a first name + last name to generate employees.

```json
{
  "maleFirstNames": ["James", "John", "Robert", ...],
  "femaleFirstNames": ["Mary", "Patricia", "Jennifer", ...],
  "lastNames": ["Smith", "Johnson", "Williams", ...]
}
```

**To customize:**
- Add or remove names from any array
- Names can be from any culture or language
- Keep at least 10 names in each array to avoid repetition
- The game handles duplicate detection automatically

---

## 2. Editing Random Events

**File:** `data/events.json`

This file contains all the text for random events that occur during gameplay. Events are organized by category.

**Categories:**
- `marketDisruptions` — Market-level events (technology shifts, regulations)
- `competitorActions` — What competitors do (price wars, poaching)
- `financialCrises` — Financial emergencies (lawsuits, equipment failure)
- `scandals` — Reputation-damaging events
- `mismanagements` — Internal management failures
- `positivePR` — Good news events
- `catastrophicEvents` — Major disasters
- `randomChaos` — Unpredictable wild card events
- `crisisTypes` — Multi-quarter crisis definitions

**To customize:**
- Add new event descriptions to any category
- Remove events you don't want
- Edit existing text to change the tone or theme
- The game randomly selects from each array, so more entries = more variety

---

## 3. Editing Game Balance

**File:** `data/gamebalance.json`

This is the most impactful file for gameplay. It controls starting values, costs, revenue formulas, and event probabilities.

**Key sections:**

```json
{
  "startingValues": {
    "capital": 500000,        // Starting money
    "reputation": 10,         // Starting reputation (-100 to 100)
    "morale": 50,             // Starting morale (-100 to 100)
    "marketShare": 5.0,       // Starting market share (%)
    "risk": 10                // Starting risk (0 to 100)
  }
}
```

**To make the game easier:** Increase `capital`, increase `morale`, decrease `risk`.
**To make the game harder:** Decrease `capital`, decrease `reputation`, increase `risk`.

**Event probabilities** (0.0 = never, 1.0 = every quarter):
```json
{
  "chaosEngine": {
    "scandalChance": 0.15,          // 15% chance per quarter
    "catastrophicBaseChance": 0.02   // 2% base chance
  }
}
```

---

## 4. Editing Job Descriptions

**File:** `data/positions.json`

Controls what job descriptions and skill keywords appear on employee cards.

```json
{
  "departments": {
    "Marketing": {
      "descriptions": [
        "Brand strategist with creative campaign experience",
        "Digital marketing specialist focused on social media growth"
      ],
      "keywords": ["campaigns", "branding", "social media", "analytics"]
    }
  }
}
```

**To customize:**
- Add new descriptions for any department
- Add or change skill keywords
- Department names must match exactly: Marketing, Operations, Finance, HR, IT, Research

---

## 5. Changing the Visual Theme

All visual styling is in the `styles/` folder as XAML resource dictionaries.

**Colors are defined as hex codes.** The game uses a dark theme with these base colors:
- Background: `#1a1a2e` (dark navy)
- Panels: `#16213e` (slightly lighter navy)
- Borders: `#2a3a5e` (muted blue)
- Accent: `#4a7c59` (green for positive), `#7c4a4a` (red for danger)

To create a light theme, you would change these hex values across the style files.

---

## 6. Replacing Images

Images are PNG files in the `images/` directory. Replace any PNG with your own image of the same filename and dimensions.

- **Department icons** (`images/marketing.png`, etc.) — Used as department tile backgrounds
- **Employee portraits** (`images/emp_male/emp1.png` through `emp10.png`, `images/emp_female/efp1.png` through `efp10.png`) — Randomly assigned to employees
- **Character portraits** (`images/char/*.png`) — Story mode character faces
- **Logo** (`images/logo.png`) — Main menu logo

---

## 7. Replacing Background Music

Replace `audio/background.mp3` with any MP3 file. The game loops it automatically. Keep the filename the same.

---

## Important Notes

- Always keep a backup of original files before editing
- JSON files must be valid JSON — use a JSON validator if unsure
- The game falls back to hardcoded defaults if a data file is missing or corrupted
- After editing, restart the game to see changes (no hot-reload)
- XAML changes require rebuilding the project (`dotnet build`)
- JSON data file changes do NOT require rebuilding — just restart the game
