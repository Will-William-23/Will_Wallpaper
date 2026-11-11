@echo off
setlocal
set SCRIPT_DIR=%~dp0

echo 1. Compilando ejecutable mas reciente...
call "%SCRIPT_DIR%build.bat"
if %ERRORLEVEL% neq 0 (
    echo Error durante la compilacion de WillWallpaper.exe
    exit /b 1
)

echo.
echo 2. Buscando Inno Setup Compiler (ISCC.exe)...

set ISCC=
if exist "%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe" set ISCC=%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe
if "%ISCC%"=="" if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" set ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe
if "%ISCC%"=="" if exist "%ProgramFiles%\Inno Setup 6\ISCC.exe" set ISCC=%ProgramFiles%\Inno Setup 6\ISCC.exe

if "%ISCC%"=="" (
    echo No se encontro Inno Setup instalado.
    echo Puedes instalarlo ejecutando: winget install JRSoftware.InnoSetup
    exit /b 1
)

echo Usando compilador: %ISCC%
echo.
echo 3. Creando instalador (comprimiendo mpv.exe)...
"%ISCC%" "%SCRIPT_DIR%setup.iss"

if %ERRORLEVEL% equ 0 (
    echo.
    echo ========================================================
    echo  Instalador generado exitosamente en:
    echo  %SCRIPT_DIR%..\..\Instalador\WillWallpaper_Setup.exe
    echo ========================================================
    exit /b 0
) else (
    echo Error al empaquetar el instalador.
    exit /b 1
)
