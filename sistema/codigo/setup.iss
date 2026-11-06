; Script de Inno Setup para Will Wallpaper
#define MyAppName "Will Wallpaper"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Will"
#define MyAppExeName "WillWallpaper.exe"
#define RootDir "..\..\"

[Setup]
AppId={{E68BC208-88D8-4C81-A528-912A34691234}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir={#RootDir}Instalador
OutputBaseFilename=WillWallpaper_Setup
SetupIconFile={#RootDir}sistema\logo.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#RootDir}{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#RootDir}sistema\*"; DestDir: "{app}\sistema"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "codigo,codigo\*,debug.log,error.log,debug_desktop.txt"
Source: "{#RootDir}WallpapersGuardados\*"; DestDir: "{app}\WallpapersGuardados"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\sistema\logo.ico"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\sistema\logo.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

