; Script de Inno Setup para crear WillWallpaper_Setup.exe
[Setup]
AppName=Will Wallpaper
AppVersion=1.0
AppPublisher=Will William
DefaultDirName={autopf}\Will Wallpaper
DefaultGroupName=Will Wallpaper
UninstallDisplayIcon={app}\sistema\logo.ico
Compression=lzma2/max
SolidCompression=yes
OutputDir=..\
OutputBaseFilename=WillWallpaper_Setup
SetupIconFile=sistema\logo.ico
PrivilegesRequired=lowest

[Files]
Source: "WillWallpaper.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "sistema\bg.jpg"; DestDir: "{app}\sistema"; Flags: ignoreversion
Source: "sistema\logo.ico"; DestDir: "{app}\sistema"; Flags: ignoreversion
Source: "sistema\active_wallpaper.txt"; DestDir: "{app}\sistema"; Flags: ignoreversion
Source: "sistema\mpv.exe"; DestDir: "{app}\sistema"; Flags: ignoreversion
Source: "WallpapersGuardados\*"; DestDir: "{app}\WallpapersGuardados"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Will Wallpaper"; Filename: "{app}\WillWallpaper.exe"; IconFilename: "{app}\sistema\logo.ico"
Name: "{autodesktop}\Will Wallpaper"; Filename: "{app}\WillWallpaper.exe"; IconFilename: "{app}\sistema\logo.ico"

[Run]
Filename: "{app}\WillWallpaper.exe"; Description: "Ejecutar Will Wallpaper ahora"; Flags: postinstall nowait skipifsilent
