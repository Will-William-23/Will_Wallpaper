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
            // Forzar la creación de WorkerW detrás de los iconos
            SendMessageTimeout(progman, 0x052C, new IntPtr(0), IntPtr.Zero, 0x0, 1000, out result);

            IntPtr workerw = IntPtr.Zero;

            // Buscar el WorkerW hermano de SHELLDLL_DefView (Típico Windows 10/11)
            EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr shellView = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (shellView != IntPtr.Zero)
                {
                    workerw = FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", null);
                }
                return true;
            }), IntPtr.Zero);

            // Si falla, probar el truco de Windows 11 para separar el escritorio
            if (workerw == IntPtr.Zero)
            {
                SendMessageTimeout(progman, 0x052C, new IntPtr(0x0000000D), new IntPtr(0), 0x0, 1000, out result);
                SendMessageTimeout(progman, 0x052C, new IntPtr(0x0000000D), new IntPtr(1), 0x0, 1000, out result);

                EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
                {
                    IntPtr shellView = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (shellView != IntPtr.Zero)
                    {
                        workerw = FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", null);
                    }
                    return true;
                }), IntPtr.Zero);
            }

            // Fallback: Buscar cualquier WorkerW visible sin hijos
            if (workerw == IntPtr.Zero)
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
                            workerw = tophandle;
                            return false;
                        }
                    }
                    return true;
                }), IntPtr.Zero);
            }

            return (workerw != IntPtr.Zero) ? workerw : progman;
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
                mpvProcess.StartInfo.Arguments = string.Format("--wid={0} --loop=inf --no-border --mute=yes --hwdec=auto --profile=fast --media-controls=no \"{1}\"", parent.ToInt64(), targetVideo);
                mpvProcess.StartInfo.UseShellExecute = false;
                mpvProcess.StartInfo.CreateNoWindow = true;
                mpvProcess.Start();
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
