; Corporate Chaos - Inno Setup Installer Script
; Version: 0.1.2 Beta
;
; Prerequisites:
;   1. Install Inno Setup 6 from https://jrsoftware.org/isdl.php
;   2. Run: dotnet publish -c Release -r win-x64 --self-contained -o publish
;      from the corporate_chaos/ directory
;   3. Open this file in Inno Setup Compiler and click Build > Compile
;
; The installer expects the publish output at:
;   corporate_chaos/publish/

#define MyAppName "Corporate Chaos"
#define MyAppVersion "0.1.2"
#define MyAppPublisher "Theo-retical808"
#define MyAppURL "https://github.com/Theo-retical808/corpo_chaos"
#define MyAppExeName "corporate_chaos.exe"

; Path to the publish output (relative to this .iss file)
#define PublishDir "..\corporate_chaos\publish"

[Setup]
AppId={{B7E3F2A1-4C8D-4E9F-A1B2-3C4D5E6F7A8B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=output
OutputBaseFilename=CorporateChaos_v{#MyAppVersion}_Setup
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
InfoBeforeFile=InfoBefore.txt

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startmenuicon"; Description: "Create a Start Menu shortcut"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; Main application and all runtime files
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Data files marked as writable so users can customize them
Source: "{#PublishDir}\data\*"; DestDir: "{app}\data"; Flags: ignoreversion; Permissions: users-modify

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Dirs]
; Ensure game data directories exist and are writable
Name: "{app}\data"; Permissions: users-modify
Name: "{app}\game_runs"; Permissions: users-modify
Name: "{app}\sv_game"; Permissions: users-modify

[UninstallDelete]
; Clean up user-generated files on uninstall (optional saves)
Type: filesandordirs; Name: "{app}\game_runs"
Type: filesandordirs; Name: "{app}\sv_game"
Type: files; Name: "{app}\highscores.json"
Type: files; Name: "{app}\runs_history.json"
Type: files; Name: "{app}\settings.json"
Type: files; Name: "{app}\story_progress.json"
