# Will Wallpaper 🚀

Reproductor de fondos de pantalla animados (Live Wallpapers) para Windows 10 y Windows 11 (compatible con 24H2+), desarrollado en **C# (.NET Framework)** utilizando **MPV** dentro de una ventana contenedora detrás de los iconos del escritorio.

---

## ✨ Características

* **Bajo Consumo de Recursos:** Aceleración por hardware (`--hwdec=auto-safe`, `--d3d11-flip=no`) compatible con laptops de doble tarjeta gráfica (Intel/AMD + NVIDIA).
* **Compatible con Windows 10 y Windows 11 (24H2+):** Soporta tanto `WorkerW` clásico como el escritorio en capas bajo `Progman` (`WS_EX_LAYERED`).
* **Limpieza Total al Detener:** Cierra la ventana contenedora y restaura el fondo estático de Windows al instante.
* **Instalador Único:** Empaquetado profesional con Inno Setup (`WillWallpaper_Setup.exe`, sin incluir videos de relleno ni código fuente).

---

## 📁 Estructura del Proyecto

```text
📁 Will Wallpaper
│
├── 🚀 WillWallpaper.exe          # Ejecutable para pruebas locales
├── 📁 Instalador                 # Salida de WillWallpaper_Setup.exe
├── 📁 WallpapersGuardados        # Carpeta de videos guardados (.mp4, etc.)
│
└── 📁 sistema                    # Archivos internos del sistema
    ├── mpv.exe                   # Motor de video MPV (no incluido en Git)
    ├── bg.jpg                    # Imagen de fondo de la interfaz
    ├── logo.ico                  # Icono de la aplicación
    └── 📁 codigo                 # Código fuente y scripts
        ├── Program.cs
        ├── ConfigForm.cs
        ├── WallpaperManager.cs
        ├── build.bat             # Compilación rápida para pruebas locales
        ├── crear_instalador.bat  # Compila y genera el instalador Setup.exe
        └── setup.iss             # Script de Inno Setup
```

---

## 🛠️ Comandos de Compilación (BUILD)

El código se compila con el compilador nativo de C# incluido en Windows (.NET Framework 4.0+):

1. **Pruebas rápidas locales (`WillWallpaper.exe`):**
   ```cmd
   sistema\codigo\build.bat
   ```
2. **Construir instalador final (`Instalador\WillWallpaper_Setup.exe`):**
   *(Requiere [Inno Setup 6](https://jrsoftware.org/isinfo.php))*
   ```cmd
   sistema\codigo\crear_instalador.bat
   ```

> **Nota:** Por límite de tamaño de GitHub (100 MB), `mpv.exe` no está incluido en este repositorio. Descarga `mpv.exe` para Windows desde [mpv.io](https://mpv.io/) y colócalo dentro de `sistema/`.

---

## 👤 Autor

Desarrollado por **William** ([@Will-William-23](https://github.com/Will-William-23)).
