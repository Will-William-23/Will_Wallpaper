using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WillWallpaper
{
    public static class WallpaperManager
    {
        public const string StopEventName = "Local\\WillWallpaper_StopSignal";

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

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

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

        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const int WS_CHILD = 0x40000000;
        private const int WS_POPUP = unchecked((int)0x80000000);
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const uint LWA_ALPHA = 0x00000002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const int SW_HIDE = 0;
        private const int SW_SHOWNOACTIVATE = 4;

        public static string GetExeDir()
        {
            return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        }

        public static string GetWritableDataDir()
        {
            string exeDir = GetExeDir();
            try
            {
                string testDir = Path.Combine(exeDir, "sistema");
                if (!Directory.Exists(testDir)) Directory.CreateDirectory(testDir);
                string testFile = Path.Combine(testDir, ".write_test");
                File.WriteAllText(testFile, "");
                File.Delete(testFile);
                return exeDir;
            }
            catch
            {
                string appData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "WillWallpaper"
                );
                if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
                return appData;
            }
        }

        private class WallpaperHostForm : Form
        {
            public WallpaperHostForm()
            {
                this.Text = "WillWallpaperHost";
                this.FormBorderStyle = FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(-32000, -32000);
                this.Size = new Size(1, 1);
                this.BackColor = Color.Black;
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT;
                    return cp;
                }
            }

            protected override bool ShowWithoutActivation
            {
                get { return true; }
            }
        }

        private static bool AttachHostToDesktop(IntPtr hostHwnd, out string debugInfo)
        {
            IntPtr progman = FindWindow("Progman", null);
            if (progman == IntPtr.Zero)
            {
                debugInfo = "Mode: NoProgman";
                return false;
            }

            IntPtr result = IntPtr.Zero;
            SendMessageTimeout(progman, 0x052C, IntPtr.Zero, IntPtr.Zero, 0x0, 1000, out result);

            IntPtr topIconHolder = IntPtr.Zero;
            IntPtr topWorkerW = IntPtr.Zero;
            FindTopLevelDesktopWindows(out topIconHolder, out topWorkerW);

            IntPtr shellInProgman = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
            IntPtr childWorkerW = FindWindowEx(progman, IntPtr.Zero, "WorkerW", null);

            // En Windows 11 24H2+, si (0, 0) no levantó el WorkerW hijo bajo Progman, probar (0xD, 1)
            if (topWorkerW == IntPtr.Zero && childWorkerW == IntPtr.Zero)
            {
                SendMessageTimeout(progman, 0x052C, new IntPtr(0x0000000D), new IntPtr(1), 0x0, 1000, out result);
                FindTopLevelDesktopWindows(out topIconHolder, out topWorkerW);
                shellInProgman = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
                childWorkerW = FindWindowEx(progman, IntPtr.Zero, "WorkerW", null);
            }

            // Win11 24H2+ ("Raised Desktop"): SHELLDLL_DefView permanece dentro de Progman junto a un WorkerW hijo
            bool isRaised24H2 = (shellInProgman != IntPtr.Zero) && (childWorkerW != IntPtr.Zero || topIconHolder == progman);

            if (isRaised24H2)
            {
                int ex = GetWindowLong(hostHwnd, GWL_EXSTYLE);
                SetWindowLong(hostHwnd, GWL_EXSTYLE, ex | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
                SetLayeredWindowAttributes(hostHwnd, 0, 255, LWA_ALPHA);

                ConvertToChildStyle(hostHwnd);
                SetParent(hostHwnd, progman);

                int w, h;
                GetTargetDimensions(progman, out w, out h);

                // Insertar justo DETRÁS de SHELLDLL_DefView (iconos) y DELANTE del WorkerW del sistema
                SetWindowPos(hostHwnd, shellInProgman, 0, 0, w, h, SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
                if (childWorkerW != IntPtr.Zero)
                {
                    SetWindowPos(childWorkerW, hostHwnd, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                }

                debugInfo = string.Format("Mode: Win11_24H2 | Progman: 0x{0:X} | ShellView: 0x{1:X} | ChildWorkerW: 0x{2:X} | Host: 0x{3:X} | Size: {4}x{5}",
                    progman.ToInt64(), shellInProgman.ToInt64(), childWorkerW.ToInt64(), hostHwnd.ToInt64(), w, h);
                return true;
            }

            // Windows 10 / Windows 11 <= 23H2: WorkerW hermano detrás de topIconHolder
            IntPtr parent = (topWorkerW != IntPtr.Zero) ? topWorkerW : progman;
            if (topWorkerW != IntPtr.Zero)
            {
                if (topIconHolder != IntPtr.Zero)
                {
                    SetWindowPos(topWorkerW, topIconHolder, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                }
                ShowWindow(topWorkerW, SW_SHOWNOACTIVATE);
            }

            ConvertToChildStyle(hostHwnd);
            SetParent(hostHwnd, parent);

            int pw, ph;
            GetTargetDimensions(parent, out pw, out ph);

            IntPtr insertAfter = (parent == progman) ? new IntPtr(1) /* HWND_BOTTOM */ : IntPtr.Zero;
            uint flags = (parent == progman)
                ? (SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW)
                : (SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);

            SetWindowPos(hostHwnd, insertAfter, 0, 0, pw, ph, flags);

            debugInfo = string.Format("Mode: LegacyWorkerW | Parent: 0x{0:X} | IconHolder: 0x{1:X} | Host: 0x{2:X} | Size: {3}x{4}",
                parent.ToInt64(), topIconHolder.ToInt64(), hostHwnd.ToInt64(), pw, ph);
            return true;
        }

        private static void FindTopLevelDesktopWindows(out IntPtr iconHolder, out IntPtr workerW)
        {
            IntPtr foundHolder = IntPtr.Zero;
            IntPtr foundWorker = IntPtr.Zero;

            EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr shellView = FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (shellView != IntPtr.Zero)
                {
                    foundHolder = tophandle;
                    foundWorker = FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", null);
                    return false;
                }
                return true;
            }), IntPtr.Zero);

            iconHolder = foundHolder;
            workerW = foundWorker;
        }

        private static void ConvertToChildStyle(IntPtr hwnd)
        {
            int style = GetWindowLong(hwnd, GWL_STYLE);
            style = (style & ~WS_POPUP) | WS_CHILD;
            SetWindowLong(hwnd, GWL_STYLE, style);
        }

        private static void GetTargetDimensions(IntPtr hwnd, out int width, out int height)
        {
            RECT rc;
            if (GetClientRect(hwnd, out rc) && (rc.Right - rc.Left) > 0 && (rc.Bottom - rc.Top) > 0)
            {
                width = rc.Right - rc.Left;
                height = rc.Bottom - rc.Top;
            }
            else
            {
                width = SystemInformation.VirtualScreen.Width;
                height = SystemInformation.VirtualScreen.Height;
            }
        }

        private static string ResolveVideoPath(string videoPath)
        {
            string exeDir = GetExeDir();
            string dataDir = GetWritableDataDir();
            string targetVideo = videoPath;

            if (string.IsNullOrEmpty(targetVideo))
            {
                string[] candidates = new string[] {
                    Path.Combine(dataDir, "sistema", "active_wallpaper.txt"),
                    Path.Combine(exeDir, "sistema", "active_wallpaper.txt"),
                    Path.Combine(dataDir, "active_wallpaper.txt")
                };
                foreach (string cfg in candidates)
                {
                    if (File.Exists(cfg))
                    {
                        targetVideo = File.ReadAllText(cfg).Trim();
                        if (!string.IsNullOrEmpty(targetVideo)) break;
                    }
                }
            }

            if (string.IsNullOrEmpty(targetVideo)) return null;

            if (!Path.IsPathRooted(targetVideo))
            {
                string inData = Path.Combine(dataDir, targetVideo);
                string inExe = Path.Combine(exeDir, targetVideo);
                targetVideo = File.Exists(inData) ? inData : inExe;
            }

            if (File.Exists(targetVideo)) return targetVideo;

            string fname = Path.GetFileName(targetVideo);
            string[] folders = new string[] {
                Path.Combine(dataDir, "WallpapersGuardados"),
                Path.Combine(exeDir, "WallpapersGuardados")
            };
            foreach (string folder in folders)
            {
                string byName = Path.Combine(folder, fname);
                if (File.Exists(byName)) return byName;
            }

            string[] validExts = new string[] { ".mp4", ".mkv", ".avi", ".webm", ".gif", ".wmv", ".mov" };
            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder)) continue;
                foreach (string f in Directory.GetFiles(folder))
                {
                    string ext = Path.GetExtension(f).ToLowerInvariant();
                    if (Array.IndexOf(validExts, ext) >= 0) return f;
                }
            }

            return null;
        }

        public static void RunBackgroundWallpaper(string videoPath)
        {
            string exeDir = GetExeDir();
            string dataDir = GetWritableDataDir();
            WallpaperHostForm hostForm = null;
            Process mpvProcess = null;
            EventWaitHandle stopEvent = null;

            try
            {
                string targetVideo = ResolveVideoPath(videoPath);
                if (string.IsNullOrEmpty(targetVideo) || !File.Exists(targetVideo)) return;

                string mpvPath = Path.Combine(exeDir, "sistema", "mpv.exe");
                if (!File.Exists(mpvPath)) mpvPath = Path.Combine(exeDir, "mpv.exe");
                if (!File.Exists(mpvPath)) return;

                bool createdNew;
                stopEvent = new EventWaitHandle(false, EventResetMode.ManualReset, StopEventName, out createdNew);
                stopEvent.Reset();

                hostForm = new WallpaperHostForm();
                hostForm.Show();
                IntPtr hostHwnd = hostForm.Handle;

                string debugInfo;
                AttachHostToDesktop(hostHwnd, out debugInfo);

                string args = BuildMpvArgs(hostHwnd, targetVideo, true);
                mpvProcess = StartMpv(mpvPath, args, true);

                WriteDebugLog(dataDir, debugInfo, targetVideo, mpvPath, args);

                if (mpvProcess.WaitForExit(2000) && mpvProcess.ExitCode != 0)
                {
                    try
                    {
                        string err = mpvProcess.StandardError.ReadToEnd();
                        WriteTextSafe(Path.Combine(dataDir, "sistema", "error.log"), err);
                    }
                    catch { }

                    args = BuildMpvArgs(hostHwnd, targetVideo, false);
                    mpvProcess = StartMpv(mpvPath, args, false);
                }

                // Supervisar en segundo plano señal de detención o cierre inesperado de mpv
                Thread watcher = new Thread(new ThreadStart(() =>
                {
                    while (true)
                    {
                        if (stopEvent.WaitOne(250)) break;
                        if (mpvProcess == null || mpvProcess.HasExited) break;
                    }

                    try
                    {
                        if (hostForm != null && !hostForm.IsDisposed && hostForm.IsHandleCreated)
                        {
                            hostForm.BeginInvoke(new MethodInvoker(() =>
                            {
                                try { hostForm.Close(); } catch { }
                                Application.ExitThread();
                            }));
                        }
                        else
                        {
                            Application.ExitThread();
                        }
                    }
                    catch { }
                }));
                watcher.IsBackground = true;
                watcher.Start();

                Application.Run();
            }
            catch
            {
            }
            finally
            {
                try
                {
                    if (mpvProcess != null && !mpvProcess.HasExited) mpvProcess.Kill();
                }
                catch { }
                try
                {
                    if (hostForm != null && !hostForm.IsDisposed) hostForm.Dispose();
                }
                catch { }
                try
                {
                    if (stopEvent != null) stopEvent.Close();
                }
                catch { }
            }
        }

        private static string BuildMpvArgs(IntPtr hostHwnd, string targetVideo, bool useHwDec)
        {
            string hwFlag = useHwDec ? "--hwdec=auto-safe --gpu-context=d3d11" : "--hwdec=no";
            return string.Format(
                "--wid={0} --loop=inf --no-border --mute=yes --panscan=1.0 --no-osc --no-osd-bar --no-input-default-bindings --really-quiet {1} --vo=gpu --d3d11-flip=no --force-window=yes \"{2}\"",
                hostHwnd.ToInt64(), hwFlag, targetVideo
            );
        }

        private static Process StartMpv(string mpvPath, string args, bool redirectError)
        {
            Process p = new Process();
            p.StartInfo.FileName = mpvPath;
            p.StartInfo.Arguments = args;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardError = redirectError;
            p.Start();
            return p;
        }

        private static void WriteDebugLog(string dataDir, string debugInfo, string targetVideo, string mpvPath, string args)
        {
            string content = string.Format("Fecha: {0}\n{1}\nVideo: {2}\nMPV: {3}\nArgs: {4}\n",
                DateTime.Now, debugInfo, targetVideo, mpvPath, args);
            WriteTextSafe(Path.Combine(dataDir, "sistema", "debug.log"), content);
        }

        private static void WriteTextSafe(string path, string content)
        {
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(path, content);
            }
            catch { }
        }

        public static void StopCurrentWallpaper()
        {
            string currentExe = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            int currentId = Process.GetCurrentProcess().Id;

            // 1. Señalizar cierre limpio al host para destruir la ventana antes de matar procesos
            try
            {
                EventWaitHandle stopEvent = EventWaitHandle.OpenExisting(StopEventName);
                stopEvent.Set();
                stopEvent.Close();
                Thread.Sleep(350);
            }
            catch { }

            // 2. Asegurar cierre de mpv
            foreach (var process in Process.GetProcessesByName("mpv"))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(500);
                }
                catch { }
            }

            // 3. Asegurar cierre de instancias secundarias de WillWallpaper
            foreach (var process in Process.GetProcessesByName(currentExe))
            {
                if (process.Id != currentId)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(1000);
                    }
                    catch { }
                }
            }

            // 4. Si se usó WorkerW hermano en Win10/Win11 antiguo, ocultarlo para evitar restos vacíos
            IntPtr topIconHolder, topWorkerW;
            FindTopLevelDesktopWindows(out topIconHolder, out topWorkerW);
            if (topWorkerW != IntPtr.Zero)
            {
                ShowWindow(topWorkerW, SW_HIDE);
            }

            // 5. Repintar Progman y restaurar fondo de pantalla original
            try
            {
                IntPtr progman = FindWindow("Progman", null);
                if (progman != IntPtr.Zero)
                {
                    InvalidateRect(progman, IntPtr.Zero, true);
                }

                RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false);
                if (rk != null)
                {
                    string wall = rk.GetValue("Wallpaper") as string;
                    if (!string.IsNullOrEmpty(wall))
                    {
                        SystemParametersInfo(20, 0, wall, 0x2);
                    }
                }
            }
            catch { }
        }
    }
}
