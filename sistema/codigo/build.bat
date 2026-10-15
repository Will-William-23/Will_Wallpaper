@echo off
setlocal
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
set WPF_DIR=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF

set SCRIPT_DIR=%~dp0
set SISTEMA_DIR=%SCRIPT_DIR%..\
set ROOT_DIR=%SISTEMA_DIR%..\

if not exist "%CSC%" (
    echo El compilador de C# no se encuentra en %CSC%
    pause
    exit /b
)

echo Compilando WillWallpaper...
"%CSC%" /target:winexe /out:"%ROOT_DIR%WillWallpaper.exe" /win32icon:"%SISTEMA_DIR%logo.ico" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:"%WPF_DIR%\WindowsBase.dll" /r:"%WPF_DIR%\PresentationCore.dll" /r:"%WPF_DIR%\PresentationFramework.dll" /r:"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Xaml.dll" /r:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\WindowsFormsIntegration\v4.0_4.0.0.0__31bf3856ad364e35\WindowsFormsIntegration.dll" "%SCRIPT_DIR%Program.cs" "%SCRIPT_DIR%WallpaperManager.cs" "%SCRIPT_DIR%ConfigForm.cs"

if %ERRORLEVEL% equ 0 (
    echo Compilacion exitosa: %ROOT_DIR%WillWallpaper.exe
) else (
    echo Error al compilar.
)
