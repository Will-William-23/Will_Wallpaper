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
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir={#RootDir}Instalador
OutputBaseFilename=WillWallpaper_Setup
SetupIconFile={#RootDir}sistema\logo.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Dirs]
Name: "{app}\WallpapersGuardados"

[Files]
Source: "{#RootDir}{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#RootDir}sistema\mpv.exe"; DestDir: "{app}\sistema"; Flags: ignoreversion
Source: "{#RootDir}sistema\bg.jpg"; DestDir: "{app}\sistema"; Flags: ignoreversion
Source: "{#RootDir}sistema\logo.ico"; DestDir: "{app}\sistema"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\sistema\logo.ico"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\sistema\logo.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\WallpapersGuardados"
Type: filesandordirs; Name: "{app}\sistema"
Type: dirifempty; Name: "{app}"

[Code]
procedure StopRunningApp();
var
  ResultCode: Integer;
begin
  Exec('taskkill.exe', '/F /IM mpv.exe /T', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Exec('taskkill.exe', '/F /IM WillWallpaper.exe /T', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

function InitializeSetup(): Boolean;
begin
  StopRunningApp();
  Result := True;
end;

function InitializeUninstall(): Boolean;
begin
  StopRunningApp();
  RegDeleteValue(HKEY_CURRENT_USER, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Run', 'WillWallpaper');
  Result := True;
end;
