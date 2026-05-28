# Building the Corporate Chaos Installer

## Prerequisites

1. **.NET 8.0 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Inno Setup 6** — [Download](https://jrsoftware.org/isdl.php)

## Steps

### 1. Build the Release

Open a terminal in the `corporate_chaos/` directory and run:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o release/v1.4
```

This creates a self-contained single-file build at `release/v1.4/` that runs without .NET installed on the target machine.

### 2. Compile the Installer

**Option A — Inno Setup GUI:**
1. Open `installer/corporate_chaos_installer.iss` in Inno Setup Compiler
2. Click **Build > Compile** (or press Ctrl+F9)
3. The installer will be created at `installer/output/CorporateChaos_v1.4_Setup.exe`

**Option B — Command line:**
```bash
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\corporate_chaos_installer.iss
```

### 3. Distribute

The output file `CorporateChaos_v1.4_Setup.exe` is a single installer that:
- Installs to `C:\Users\<user>\AppData\Local\Programs\Corporate Chaos` (no admin required)
- Creates Start Menu and optional Desktop shortcuts
- Includes the full .NET runtime (no prerequisites for end users)
- Makes the `data/` folder writable so users can customize JSON files
- Includes a clean uninstaller

## Updating the Version

When releasing a new version, update these values in `corporate_chaos_installer.iss`:

```
#define MyAppVersion "1.5"
#define MyAppSourceDir "..\release\v1.5"
```

The `OutputBaseFilename` will automatically reflect the new version.

## What Gets Installed

```
Corporate Chaos/
├── corporate_chaos.exe    — Main game executable (single-file, self-contained)
├── data/                  — Customizable JSON data files (writable)
│   ├── events.json
│   ├── gamebalance.json
│   ├── names.json
│   └── positions.json
└── audio/                 — Background music
    └── background.mp3
```

## Changelog

### v1.4
- Interactive tutorial: highlights follow dialogue, auto-advance on button click
- Tutorial overlay always renders above modal panels (hiring, executive decisions)
- Removed hire confirmation MessageBox popup
- Expanded dialogue pool (12 unique lines per phase, no repeats)
- Smooth tutorial flow with no dead spots

### v1.3
- Improved tutorial step-by-step guidance and pacing
- Relationship handling improvements with key characters
