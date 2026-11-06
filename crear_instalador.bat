@echo off
set ISCC="C:\Program Files (x86)\Inno Setup 6\iscc.exe"

if not exist %ISCC% (
    echo [ERROR] Inno Setup no esta instalado.
    echo.
    echo Para instalarlo abre PowerShell o CMD y ejecuta:
    echo winget install JRSoftware.InnoSetup
    echo.
    pause
    exit /b
)

echo Generando instalador WillWallpaper_Setup.exe...
%ISCC% "%~dp0installer.iss"

if %ERRORLEVEL% equ 0 (
    echo.
    echo ========================================================
    echo [EXITO] Instalador creado en carpeta superior:
    echo WillWallpaper_Setup.exe
    echo ========================================================
) else (
    echo.
    echo [ERROR] Fallo la creacion del instalador.
)
pause
