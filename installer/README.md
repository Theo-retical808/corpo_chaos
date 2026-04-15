# Building the Corporate Chaos Installer

## Prerequisites

1. **.NET 8.0 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Inno Setup 6** — [Download](https://jrsoftware.org/isdl.php)

## Steps

### 1. Build the Release

Open a terminal in the `corporate_chaos/` directory and run:

```bash
dotnet publish -c Release -r win-x64 --self-contained -o publish
```

This creates a self-contained build at `corporate_chaos/publish/` (~170MB) that runs without .NET installed on the target machine.

### 2. Compile the Installer

**Option A — Inno Setup GUI:**
1. Open `installer/CorporateChaosSetup.iss` in Inno Setup Compiler
2. Click **Build > Compile** (or press Ctrl+F9)
3. The installer will be created at `installer/output/CorporateChaos_v0.1.2_Setup.exe`

**Option B — Command line:**
```bash
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\CorporateChaosSetup.iss
```

### 3. Distribute

The output file `CorporateChaos_v0.1.2_Setup.exe` is a single installer that:
- Installs to `C:\Users\<user>\AppData\Local\Programs\Corporate Chaos` (no admin required)
- Creates Start Menu and optional Desktop shortcuts
- Includes the full .NET runtime (no prerequisites for end users)
- Makes the `data/` folder writable so users can customize JSON files
- Includes a clean uninstaller

## Updating the Version

When releasing a new version, update these values in `CorporateChaosSetup.iss`:

```
#define MyAppVersion "0.1.3"
```

And update `OutputBaseFilename` will automatically reflect the new version.

## What Gets Installed

```
Corporate Chaos/
├── corporate_chaos.exe    — Main game executable
├── data/                  — Customizable JSON data files (writable)
├── audio/                 — Background music
├── game_runs/             — Sandbox save files (created at runtime)
├── sv_game/               — Story mode saves (created at runtime)
├── *.dll                  — .NET runtime and WPF libraries
└── [locale folders]       — .NET localization resources
```
