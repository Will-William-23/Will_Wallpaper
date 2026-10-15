# Will Wallpaper 🚀

Reproductor de fondos de pantalla animados (Live Wallpapers) para Windows, ultra liviano, eficiente y 100% portable. Desarrollado en **C# (.NET Framework)** utilizando el motor **MPV** para renderizar videos directamente detrás de los iconos del escritorio.

---

## ✨ Características

* **Bajo Consumo de Recursos:** Consume mínima memoria RAM y aprovecha aceleración por hardware (`--hwdec=auto`, `--profile=fast`).
* **Integración Nativa con Windows:** Inyección directa en la ventana `WorkerW` / `Progman` de Windows 10 y 11.
* **Interfaz Moderna:** Diseño oscuro con efecto translúcido, vista previa del video y controles intuitivos.
* **Inicio Automático Optimizado:** Opción de iniciar con Windows eliminando el retraso habitual de inicio (`StartupDelayInMSec`).
* **Sin Controles Fantasma:** Bloquea los controles multimedia en la pantalla de bloqueo (SMTC) para que no interfiera con otros reproductores.
* **100% Portable:** Guarda rutas relativas, permitiendo mover la carpeta completa o usarla desde una memoria USB.

---

## 📁 Estructura del Proyecto

```text
📁 Will Wallpaper
│
├── 🚀 WillWallpaper.exe          # Ejecutable principal
├── 📁 WallpapersGuardados        # Carpeta donde se guardan tus videos (.mp4)
│
└── 📁 sistema                    # Archivos internos del sistema
    ├── mpv.exe                   # Motor de video MPV
    ├── bg.jpg                    # Imagen de fondo de la interfaz
    ├── logo.ico                  # Icono de la aplicación
    ├── active_wallpaper.txt      # Archivo de configuración del fondo activo
    └── 📁 codigo                 # Código fuente del proyecto
        ├── ConfigForm.cs
        ├── Program.cs
        ├── WallpaperManager.cs
        └── build.bat             # Script de compilación
```

---

## 🛠️ Compilación

El proyecto no requiere dependencias pesadas ni Visual Studio completo; se compila utilizando el compilador nativo de C# incluido en Windows (.NET Framework 4.0+):

1. Abre una terminal o ejecuta directamente:
   ```cmd
   sistema\codigo\build.bat
   ```
2. El script generará automáticamente el ejecutable `WillWallpaper.exe` en la raíz del proyecto.

> **Nota:** Debido al límite de tamaño de archivos en GitHub (100 MB), `mpv.exe` no está incluido en este repositorio. Puedes descargar la versión de Windows de [mpv.io](https://mpv.io/) y colocar el archivo `mpv.exe` dentro de la carpeta `sistema/`.

---

## 👤 Autor

Desarrollado por **William** ([@Will-William-23](https://github.com/Will-William-23)).
