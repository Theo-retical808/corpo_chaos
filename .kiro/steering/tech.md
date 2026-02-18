# Corporate Chaos - Technical Stack

## Framework & Platform
- **Framework**: .NET 8.0 Windows (WPF Application)
- **Language**: C# with nullable reference types enabled
- **UI Framework**: Windows Presentation Foundation (WPF) with XAML
- **Target Platform**: Windows desktop application

## Project Structure
- **SDK Style Project**: Uses `Microsoft.NET.Sdk` with modern project format
- **Output Type**: Windows executable (`WinExe`)
- **Implicit Usings**: Enabled for cleaner code

## Dependencies & Libraries
- **Core**: .NET 8.0 Windows runtime
- **UI**: WPF framework (built-in)
- **Serialization**: System.Text.Json for save/load functionality
- **Audio**: Built-in Windows media capabilities for background music

## Build & Development Commands

### Building the Application
```bash
# Build the project
dotnet build

# Build for release
dotnet build -c Release

# Run the application
dotnet run

# Clean build artifacts
dotnet clean
```

### Project Management
```bash
# Restore NuGet packages
dotnet restore

# Publish for deployment
dotnet publish -c Release -r win-x64 --self-contained
```

## Resource Management
- **Images**: PNG files in `/images` folder, embedded as resources
- **Audio**: MP3 files in `/audio` folder, copied to output directory
- **Data**: JSON files for game saves, high scores, and configuration

## Architecture Notes
- Uses MVVM-adjacent pattern with code-behind for UI logic
- Event-driven architecture for UI updates and game state changes
- Modular system design with separate classes for game logic (ChaosEngine, DecisionSystem, etc.)
- JSON serialization for persistent data storage