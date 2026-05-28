# Changelog

All notable changes to Corporate Chaos are documented here.

## v1.4 (2026-05-29)

### Tutorial System Overhaul
- **Interactive flow**: Tutorial highlights now follow exactly what Joan is saying in each dialogue line. When she mentions "Hire New Employees", the Hire button glows.
- **Auto-advance on interaction**: Steps with a highlighted button advance automatically when the player clicks that button. No more manual "Got it" clicking during guided steps.
- **Always on top**: Tutorial dialogue overlay renders above all game panels including modal windows (hiring panel, executive decisions). Players never lose sight of Joan's guidance.
- **No dead spots**: Eliminated scenarios where the tutorial showed nothing and players didn't know what to do next. Context steps show a clear "Next →" button; action steps show the highlighted target.
- **Linear flow**: Removed back button for cleaner progression. Tutorial is now a smooth guided experience from start to finish.

### Dialogue System
- **12 unique lines per phase**: Expanded Joan's dialogue from 3-5 options to 12 per relationship phase (Professional, Trusted, Personal, Lifelong).
- **No repeats**: Static tracking ensures every dialogue line is shown before any can repeat. Resets only after the full pool is exhausted.
- **Expanded strategic advice**: 12 unique strategic advice lines (up from 5).
- **Expanded end-of-quarter messages**: 12 unique generic messages (up from 5).

### UX Improvements
- **Removed hire alert popup**: The MessageBox confirmation when hiring an employee has been removed. Hiring is now seamless — the event log still records it.
- **Smoother End Quarter during tutorial**: If End Quarter is the tutorial's final step, clicking it completes the tutorial AND processes the quarter in one action.

## v1.3 (2026-05-29)

### Tutorial
- Clearer step-by-step guidance with contextual hints
- Better pacing throughout the early game tutorial (Q1-Q10)

### Relationships
- Key character relationships update more reliably across quarters
- Improved phase transitions and dialogue triggers
- Relationship-aware dialogue adapts to trust/respect/connection levels

### Technical
- Version stamped in assembly metadata (Version, AssemblyVersion, FileVersion)
- Inno Setup installer with self-contained single-file deployment
- Release notes embedded in project metadata
