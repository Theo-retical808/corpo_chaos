# Corporate Chaos - Project Structure

## Root Directory Structure
```
corporate_chaos/                 # Main application directory
├── models/                      # Data models and business entities
├── systems/                     # Core game logic and engines
├── views/                       # UI windows and user controls
├── viewModels/                  # (Currently empty - reserved for MVVM)
├── images/                      # UI icons and graphics (PNG)
├── audio/                       # Background music and sound effects
├── game_runs/                   # Saved game run data (JSON)
├── sv_game/                     # Story mode save files
├── *.xaml + *.xaml.cs          # Main window and app files
├── *.csproj                     # Project configuration
└── *.md                         # Documentation files
```

## Code Organization Patterns

### Models (`/models/`)
- **Company.cs**: Core business entity with financial stats, settings, and game logic
- **Employee.cs**: Employee entities with skills, departments, and productivity
- **GameSave.cs**: Serializable save game data structure
- **GameScore.cs**: High score and performance tracking
- **StoryMode.cs**: Story mode progression and tutorial state

### Systems (`/systems/`)
- **ChaosEngine.cs**: Random event generation and crisis management
- **DecisionSystem.cs**: Executive decision processing
- **DataManager.cs**: Configuration and data persistence
- **SaveLoadManager.cs**: Game save/load functionality
- **StoryModeManager.cs**: Tutorial progression and guidance
- **BackgroundMusicManager.cs**: Audio system management

### Views (`/views/`)
- **DepartmentPanel.xaml/.cs**: Employee assignment and department management
- **ExecutiveDecisions.xaml/.cs**: Strategic decision interface
- **HiringPanel.xaml/.cs**: Employee recruitment system
- **QuarterlySummary.xaml/.cs**: Performance review and analytics
- **JoanDialogue.xaml/.cs**: Story mode tutorial dialogs
- **HighScoresWindow.xaml/.cs**: Leaderboard and achievements
- **SaveFileManager.xaml/.cs**: Save game management

## Naming Conventions
- **Classes**: PascalCase (e.g., `ChaosEngine`, `DepartmentPanel`)
- **Methods**: PascalCase (e.g., `ProcessQuarterlyFinancials`)
- **Properties**: PascalCase (e.g., `QuarterlyRevenue`)
- **Fields**: camelCase with underscore prefix for private (e.g., `_random`)
- **Enums**: PascalCase for both enum and values (e.g., `Department.Marketing`)
- **XAML Elements**: PascalCase with descriptive suffixes (e.g., `SaveGameBtn`, `QuarterCounterText`)

## File Naming Patterns
- **XAML Windows**: `[WindowName].xaml` + `[WindowName].xaml.cs`
- **Models**: `[EntityName].cs` (singular)
- **Systems**: `[SystemName].cs` or `[SystemName]Manager.cs`
- **Documentation**: `[TOPIC_NAME].md` (uppercase with underscores)
- **Save Files**: `[GameType]_Q[Quarter]_[Date]_[Timestamp].json`

## Data Flow Architecture
1. **UI Layer**: XAML views handle user interaction
2. **Logic Layer**: Systems process game mechanics and rules
3. **Data Layer**: Models represent game state and entities
4. **Persistence**: JSON serialization for saves and configuration

## Key Architectural Principles
- **Separation of Concerns**: UI, business logic, and data are clearly separated
- **Event-Driven Updates**: UI responds to game state changes via events
- **Modular Systems**: Each system (Chaos, Decisions, etc.) is self-contained
- **Progressive Unlocking**: Story mode gradually introduces features
- **Dynamic Scaling**: Costs and difficulty scale with game progression