# Corporate Chaos — Technical Decisions & Architecture

## 1. Programming Language: C#

### Why C#

C# was chosen as the primary language for Corporate Chaos for several practical reasons:

- **Strong typing with flexibility.** C# catches a wide class of bugs at compile time through its type system, nullable reference types, and enums. For a simulation game with dozens of interconnected stats, formulas, and state transitions, this prevents entire categories of runtime errors that would be painful to debug in a dynamically typed language.
- **First-class JSON serialization.** `System.Text.Json` is built into the runtime — no third-party dependencies needed. Every save file, configuration file, and data file in the game uses JSON, and C# handles serialization/deserialization with simple attributes (`[JsonPropertyName]`). This is critical for the offline and customizable goals.
- **Mature ecosystem for desktop applications.** C# has decades of tooling, documentation, and community support for building Windows desktop software. The language itself is well-documented by Microsoft and widely taught, which lowers the barrier for contributors who want to modify the game.
- **Performance.** C# compiles to native code via .NET's JIT/AOT compilation. For a turn-based game this is more than sufficient, but it also means the game starts fast and runs smoothly even on older hardware.

### Alternatives Considered

| Language | Why Not |
|----------|---------|
| **Python + Tkinter/PyQt** | Slower startup, requires Python runtime installed, packaging for distribution is messy (PyInstaller bundles are large and fragile). |
| **JavaScript + Electron** | Massive memory footprint (~200MB+ baseline), requires bundling an entire Chromium browser. Contradicts the "lightweight offline app" goal. |
| **C++ + Qt** | Powerful but significantly higher development complexity. Manual memory management adds risk for a project that prioritizes rapid iteration and modifiability. |
| **Java + JavaFX** | Requires JRE installation. JavaFX distribution story is complicated since Oracle decoupled it from the JDK. |

C# hits the sweet spot: compiled, fast, strongly typed, and the runtime is either pre-installed on Windows or can be bundled as a self-contained publish.

---

## 2. Framework: .NET 8.0 with WPF

### Why .NET 8.0

- **Long-Term Support (LTS).** .NET 8.0 is an LTS release supported by Microsoft through November 2026. This means security patches and bug fixes without forced major version upgrades.
- **Zero external dependencies.** The game uses only what ships with the .NET SDK — no NuGet packages. The entire dependency list is: `Microsoft.NET.Sdk` with `UseWPF`. This makes the project trivial to build on any machine with the .NET 8 SDK installed.
- **Self-contained deployment.** `dotnet publish -r win-x64 --self-contained` produces a single folder that runs on any Windows machine without requiring .NET to be installed. This directly supports the offline distribution goal.
- **Modern C# features.** .NET 8 supports C# 12, giving access to pattern matching, nullable reference types, implicit usings, file-scoped namespaces, and other features that keep the codebase clean.

### Why WPF (Windows Presentation Foundation)

WPF was chosen over other UI frameworks for these reasons:

- **XAML declarative UI.** The entire UI is defined in `.xaml` files separate from the logic in `.xaml.cs` code-behind files. This separation means someone can reskin the game by editing XAML without touching any C# code. Modders can change colors, layouts, fonts, and add new UI elements through XML editing alone.
- **Rich styling system.** WPF's `ResourceDictionary` and `Style` system allows defining reusable visual themes. Corporate Chaos uses this extensively — `ButtonStyles.xaml`, `GameTheme.xaml`, `Typography.xaml`, `ScrollBarStyles.xaml` are all swappable theme files. A modder could create an entirely different visual theme by replacing these files.
- **Data binding.** WPF's binding system connects UI elements directly to data properties. Employee lists, department stats, and financial displays all use data binding, which keeps the UI automatically synchronized with game state.
- **No web server, no browser, no network.** WPF runs as a native Windows process. There is no localhost server, no HTTP stack, no WebSocket connection. The game is a single process that reads and writes local files. This is the simplest possible architecture for an offline desktop game.
- **Built into .NET.** WPF ships with the .NET Windows SDK. No additional downloads, no package manager entries, no version conflicts.

### Alternatives Considered

| Framework | Why Not |
|-----------|---------|
| **WinForms** | Simpler but much more limited styling. No XAML, no resource dictionaries, no data templates. Customization would require code changes rather than XML edits. |
| **MAUI** | Cross-platform but immature for desktop. WPF is battle-tested for Windows desktop; MAUI adds complexity for platforms we don't target. |
| **Avalonia UI** | Cross-platform XAML framework. Promising, but smaller community and less documentation. Would be a strong choice if Linux/macOS support becomes a goal. |
| **Unity** | Full game engine — massive overkill for a turn-based management sim with no real-time rendering. Adds hundreds of megabytes to the build and requires learning Unity's paradigms. |
| **Godot** | Lighter than Unity but still a full game engine. The UI system is less flexible than WPF for form-heavy interfaces like financial reports and employee management panels. |

WPF is the right tool for this specific job: a data-heavy, form-heavy, turn-based simulation that needs to look polished and be easily customizable, running exclusively on Windows.

---

## 3. Architecture

### Overview

Corporate Chaos follows a **layered architecture** with clear separation between data, logic, and presentation:

```
┌─────────────────────────────────────────────────┐
│                  PRESENTATION                    │
│  MainWindow.xaml    views/*.xaml    styles/*.xaml │
│  (XAML + code-behind)                            │
├─────────────────────────────────────────────────┤
│                  GAME SYSTEMS                    │
│  ChaosEngine    DecisionSystem    StoryMode      │
│  NarrativeEngine    SaveLoadManager    etc.       │
├─────────────────────────────────────────────────┤
│                  DATA MODELS                     │
│  Company    Employee    GameSave    GameScore     │
├─────────────────────────────────────────────────┤
│              EXTERNAL DATA (JSON)                │
│  data/names.json    data/events.json             │
│  data/gamebalance.json    data/positions.json    │
│  game_runs/*.json    settings.json               │
└─────────────────────────────────────────────────┘
```

### Layer Responsibilities

**Presentation Layer** (`views/`, `styles/`, `MainWindow.xaml`)
- All UI rendering and user interaction
- XAML defines layout and visual design
- Code-behind handles event wiring and UI updates
- Style resource dictionaries define the visual theme

**Systems Layer** (`systems/`)
- All game logic lives here
- Each system is a self-contained class with a single responsibility
- Systems operate on models and return results — they don't touch the UI directly
- Key systems: `ChaosEngine` (random events), `DecisionSystem` (executive actions), `StoryModeManager` (narrative progression), `SaveLoadManager` (persistence), `GameDataLoader` (JSON data loading)

**Models Layer** (`models/`)
- Plain C# classes with JSON serialization attributes
- Represent game state: company stats, employees, save data, scores
- No logic beyond simple calculations (e.g., `GetQuarterlyCost()`)
- Serializable to/from JSON for save/load

**External Data Layer** (`data/`)
- JSON files containing all configurable game parameters
- Employee names, event descriptions, balance values, position templates
- Loaded at runtime by `GameDataLoader`
- Editable by anyone with a text editor — no recompilation needed

### Why This Architecture

**Separation of concerns.** Each layer has a clear job. You can change the UI without touching game logic. You can rebalance the game by editing JSON files without touching any code. You can add new event types by editing `data/events.json`.

**Modifiability over abstraction.** The architecture deliberately avoids heavy abstractions like dependency injection containers, service locators, or plugin systems. The codebase is straightforward: classes instantiate their dependencies directly, systems are created in `MainWindow.xaml.cs`, and data flows through method parameters. This makes the code easy to read and modify for someone who isn't a professional C# developer.

**Event-driven UI updates.** When game state changes, the UI is updated through explicit method calls (`UpdateUI()`, `RefreshEmployeeLists()`). This is simpler than full MVVM data binding for a game where state changes happen at discrete points (end of quarter) rather than continuously.

---

## 4. Data-Driven Design

### The Customization Goal

The primary design goal is that **anyone can customize the game without writing code**. This is achieved through JSON data files:

| File | What It Controls |
|------|-----------------|
| `data/names.json` | Male first names, female first names, last names for employee generation |
| `data/events.json` | All random event descriptions: market disruptions, scandals, crises, positive PR, chaos events, etc. |
| `data/gamebalance.json` | Starting capital, department budgets, risk/investment multipliers, quarterly financial formulas, chaos engine probabilities, turnover rates, market dynamics |
| `data/positions.json` | Department-specific job descriptions and skill keywords |
| `settings.json` | Player preferences (audio volume, display mode) |

### How It Works

`GameDataLoader` loads JSON files at runtime with caching and fallback defaults:

1. On first access, it reads the JSON file from the `data/` directory
2. The parsed data is cached in memory for the rest of the session
3. If the file is missing or corrupted, hardcoded fallback defaults are used
4. The game never crashes due to missing data files

This means a modder can:
- Add 500 new employee names by editing `names.json`
- Create entirely new crisis events by editing `events.json`
- Double the starting capital by changing one number in `gamebalance.json`
- Add new job descriptions by editing `positions.json`

No compilation, no IDE, no programming knowledge required.

---

## 5. Pros and Cons

### Pros

| Advantage | Detail |
|-----------|--------|
| **Completely offline** | No network calls, no telemetry, no cloud saves. The game is a single process reading/writing local files. |
| **Zero dependencies** | Only the .NET 8 SDK is needed to build. No NuGet packages, no npm, no package managers beyond dotnet itself. |
| **Customizable without code** | JSON data files control names, events, balance, and positions. XAML files control the visual theme. |
| **Small footprint** | The built application is under 20MB. Self-contained publish is ~150MB (includes .NET runtime). |
| **Fast iteration** | `dotnet build` takes under 10 seconds. `dotnet run` launches the game immediately. |
| **Readable codebase** | No frameworks, no abstractions, no magic. A C# beginner can read the code and understand what it does. |
| **Portable saves** | All game data is JSON. Save files can be copied, shared, backed up, or inspected with any text editor. |

### Cons

| Limitation | Detail |
|------------|--------|
| **Windows only** | WPF is a Windows-exclusive framework. The game cannot run on macOS or Linux without a full rewrite of the UI layer (e.g., migrating to Avalonia UI). |
| **Code-behind over MVVM** | The project uses code-behind (`*.xaml.cs`) rather than full MVVM with ViewModels and commands. This is simpler to understand but means UI logic is coupled to specific XAML elements. Refactoring the UI requires updating both files. |
| **No plugin system** | Customization is limited to editing data files and XAML. There's no way to add new game mechanics without modifying C# source code. A plugin/mod API would require significant architectural work. |
| **Single-threaded UI** | All game logic runs on the UI thread. This is fine for a turn-based game but means heavy calculations (if any were added) could freeze the UI. |
| **Manual UI updates** | Without full MVVM binding, UI updates require explicit method calls. Adding a new stat display means updating both the XAML and the `UpdateUI()` method. |

### Why These Tradeoffs Are Acceptable

The cons are deliberate tradeoffs aligned with the project goals:

- **Windows only** is acceptable because the target audience is Windows desktop users. If cross-platform becomes a priority, the models and systems layers are platform-agnostic — only the views layer would need replacement.
- **Code-behind over MVVM** keeps the learning curve low. A contributor doesn't need to understand ICommand, INotifyPropertyChanged, RelayCommand, or dependency injection to make changes. They see a button click handler, they understand it.
- **No plugin system** keeps the codebase simple. The JSON data files cover the most common customization needs. Source code modification is the escape hatch for deeper changes, and the code is structured to make that straightforward.

---

## 6. Project Structure

```
corporate_chaos/
├── data/                    # Editable JSON data files (names, events, balance)
├── models/                  # Data classes (Company, Employee, GameSave, etc.)
├── systems/                 # Game logic (ChaosEngine, DecisionSystem, etc.)
├── views/                   # UI windows (XAML + code-behind)
├── styles/                  # Visual theme (ButtonStyles, GameTheme, Typography)
├── converters/              # WPF value converters
├── images/                  # PNG assets (logos, departments, characters, employees)
├── audio/                   # Background music (MP3)
├── game_runs/               # Sandbox mode save files (JSON)
├── sv_game/                 # Story mode save files (JSON)
├── MainWindow.xaml/.cs      # Main game window and primary controller
├── App.xaml/.cs             # Application entry point
└── corporate_chaos.csproj   # Project configuration (zero NuGet dependencies)
```

---

## 7. Build & Run

```bash
# Prerequisites: .NET 8.0 SDK

# Build
dotnet build

# Run
dotnet run

# Publish self-contained (no .NET required on target machine)
dotnet publish -c Release -r win-x64 --self-contained
```

No other tools, package managers, or setup steps required.
