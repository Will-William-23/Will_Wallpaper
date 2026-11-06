using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WillWallpaper
{
    public static class WallpaperManager
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessageTimeout(IntPtr windowHandle, uint Msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        public static IntPtr GetWallpaperParent()
        {
            IntPtr progman = FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;

            // Enviar mensajes para forzar separación de capas (Windows 10 y Windows 11)
            SendMessageTimeout(progman, 0x052C, new IntPtr(0x0000000D), new IntPtr(1), 0x0, 1000, out result);
            SendMessageTimeout(progman, 0x052C, new IntPtr(0), IntPtr.Zero, 0x0, 1000, out result);

            IntPtr workerw = IntPtr.Zero;

            // Buscar el WorkerW hermano detrás de SHELLDLL_DefView
            EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr shellView = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (shellView != IntPtr.Zero)
                {
                    workerw = FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", null);
                }
                return true;
            }), IntPtr.Zero);

            // Si no se encontró o no es visible, buscar el WorkerW con dimensiones reales de pantalla
            if (workerw == IntPtr.Zero || !IsWindowVisible(workerw))
            {
                EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
                {
                    StringBuilder className = new StringBuilder(256);
                    GetClassName(tophandle, className, className.Capacity);
                    if (className.ToString() == "WorkerW")
                    {
                        IntPtr shellView = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", null);
                        if (shellView == IntPtr.Zero)
                        {
                            RECT rc;
                            if (GetClientRect(tophandle, out rc) && (rc.Right - rc.Left) > 300 && (rc.Bottom - rc.Top) > 300)
                            {
                                workerw = tophandle;
                                return false;
                            }
                        }
                    }
                    return true;
                }), IntPtr.Zero);
            }

            IntPtr target = (workerw != IntPtr.Zero) ? workerw : progman;
            if (target != IntPtr.Zero)
            {
                try
                {
                    ShowWindow(target, 5); // SW_SHOW: forzar visibilidad de la capa
                }
                catch { }
            }

            return target;
        }

        public static void RunBackgroundWallpaper(string videoPath)
        {
            string exeDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            try
            {
                string targetVideo = videoPath;
                if (string.IsNullOrEmpty(targetVideo))
                {
                    string configFile = Path.Combine(exeDir, "sistema", "active_wallpaper.txt");
                    if (!File.Exists(configFile)) configFile = Path.Combine(exeDir, "active_wallpaper.txt");
                    if (File.Exists(configFile)) targetVideo = File.ReadAllText(configFile).Trim();
                }

                if (!string.IsNullOrEmpty(targetVideo))
                {
                    if (!Path.IsPathRooted(targetVideo))
                    {
                        targetVideo = Path.Combine(exeDir, targetVideo);
                    }

                    if (!File.Exists(targetVideo))
                    {
                        string fname = Path.GetFileName(targetVideo);
                        string fallback = Path.Combine(exeDir, "WallpapersGuardados", fname);
                        if (File.Exists(fallback))
                        {
                            targetVideo = fallback;
                        }
                        else
                        {
                            string safeFolder = Path.Combine(exeDir, "WallpapersGuardados");
                            if (Directory.Exists(safeFolder))
                            {
                                string[] validExts = new string[] { ".mp4", ".mkv", ".avi", ".webm", ".gif", ".wmv", ".mov" };
                                foreach (string f in Directory.GetFiles(safeFolder))
                                {
                                    string ext = Path.GetExtension(f).ToLowerInvariant();
                                    if (Array.IndexOf(validExts, ext) >= 0)
                                    {
                                        targetVideo = f;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(targetVideo) || !File.Exists(targetVideo))
                {
                    return;
                }

                IntPtr parent = GetWallpaperParent();
                string mpvPath = Path.Combine(exeDir, "sistema", "mpv.exe");
                if (!File.Exists(mpvPath)) mpvPath = Path.Combine(exeDir, "mpv.exe");
                
                if (!File.Exists(mpvPath)) {
                    return;
                }

                Process mpvProcess = new Process();
                mpvProcess.StartInfo.FileName = mpvPath;
                mpvProcess.StartInfo.Arguments = string.Format("--wid={0} --loop=inf --no-border --mute=yes --hwdec=auto-safe --vo=gpu --gpu-context=auto --force-window=yes --media-controls=no \"{1}\"", parent.ToInt64(), targetVideo);
                mpvProcess.StartInfo.UseShellExecute = false;
                mpvProcess.StartInfo.CreateNoWindow = true;
                mpvProcess.StartInfo.RedirectStandardError = true;

                string debugFile = Path.Combine(exeDir, "sistema", "debug.log");
                try {
                    File.WriteAllText(debugFile, string.Format("Fecha: {0}\nParent HWND: {1} (0x{1:X})\nVideo: {2}\nMPV: {3}\nArgs: {4}\n",
                        DateTime.Now, parent.ToInt64(), targetVideo, mpvPath, mpvProcess.StartInfo.Arguments));
                } catch { }

                mpvProcess.Start();

                bool exitedQuickly = mpvProcess.WaitForExit(2000);
                if (exitedQuickly && mpvProcess.ExitCode != 0)
                {
                    string err = mpvProcess.StandardError.ReadToEnd();
                    string logFile = Path.Combine(exeDir, "sistema", "error.log");
                    try { File.WriteAllText(logFile, err); } catch { }
                    MessageBox.Show("Error al reproducir fondo con mpv:\n\n" + (string.IsNullOrEmpty(err) ? "Código de salida: " + mpvProcess.ExitCode : err), "Will Wallpaper - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                mpvProcess.WaitForExit();
            }
            catch
            {
            }
        }

        public static void StopCurrentWallpaper()
        {
            string currentExe = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            int currentId = Process.GetCurrentProcess().Id;

            bool killedAny = false;
            
            // Matar a mpv primero
            foreach (var process in Process.GetProcessesByName("mpv"))
            {
                try { 
                    process.Kill();
                } catch { }
            }

            foreach (var process in Process.GetProcessesByName(currentExe))
            {
                if (process.Id != currentId)
                {
                    try { 
                        process.Kill();
                        process.WaitForExit(1000); 
                        killedAny = true;
                    } catch { }
                }
            }

            if (killedAny)
            {
                try {
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false);
                    if (rk != null) {
                        string wall = rk.GetValue("Wallpaper") as string;
                        if (!string.IsNullOrEmpty(wall)) {
                            SystemParametersInfo(20, 0, wall, 0x2);
                        }
                    }
                } catch {}
            }
        }
    }
}
