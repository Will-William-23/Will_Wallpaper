using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Microsoft.Win32;
using MediaElement = System.Windows.Controls.MediaElement;
using MediaState = System.Windows.Controls.MediaState;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace WillWallpaper
{
    public class ConfigForm : Form
    {
        private Button btnVideo;
        private Button btnStop;
        private CheckBox chkStartup;
        private Label lblTitle;
        private Label lblStatus;
        private Panel topPanel;
        private Label lblDetails;
        private Panel detailsPanel;
        
        private Panel previewPanel;
        private ElementHost elementHost;
        private MediaElement previewMedia;
        private string currentPreviewPath;

        private Timer processCheckTimer;

        private bool IsValidVideoFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return ext == ".mp4" || ext == ".mkv" || ext == ".avi" || ext == ".webm" || ext == ".gif" || ext == ".wmv" || ext == ".mov";
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public ConfigForm()
        {
            this.Text = "Will Wallpaper";
            this.Size = new Size(850, 700); // Ventana más grande
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);

            string exeDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string bgPath = Path.Combine(exeDir, "sistema", "bg.jpg");
            if (!File.Exists(bgPath)) bgPath = Path.Combine(exeDir, "bg.jpg");
            if (File.Exists(bgPath))
            {
                this.BackgroundImage = Image.FromFile(bgPath);
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }

            this.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };

            topPanel = new Panel();
            topPanel.Height = 40;
            topPanel.Dock = DockStyle.Top;
            topPanel.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            topPanel.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };

            Label windowTitle = new Label();
            windowTitle.Text = "Will Wallpaper - Configuración";
            windowTitle.ForeColor = System.Drawing.Color.White;
            windowTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            windowTitle.Location = new Point(12, 10);
            windowTitle.AutoSize = true;
            topPanel.Controls.Add(windowTitle);

            // Botones de ventana
            Button btnMinimize = new Button();
            btnMinimize.Text = "—";
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.ForeColor = System.Drawing.Color.White;
            btnMinimize.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            btnMinimize.Size = new Size(40, 40);
            btnMinimize.Location = new Point(this.Width - 120, 0);
            btnMinimize.Cursor = Cursors.Hand;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = System.Drawing.Color.FromArgb(63, 63, 65);
            btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            topPanel.Controls.Add(btnMinimize);

            Button btnMaximize = new Button();
            btnMaximize.Text = "☐";
            btnMaximize.FlatStyle = FlatStyle.Flat;
            btnMaximize.FlatAppearance.BorderSize = 0;
            btnMaximize.ForeColor = System.Drawing.Color.White;
            btnMaximize.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            btnMaximize.Size = new Size(40, 40);
            btnMaximize.Location = new Point(this.Width - 80, 0);
            btnMaximize.Cursor = Cursors.Hand;
            btnMaximize.Click += (s, e) => {
                this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            };
            btnMaximize.MouseEnter += (s, e) => btnMaximize.BackColor = System.Drawing.Color.FromArgb(63, 63, 65);
            btnMaximize.MouseLeave += (s, e) => btnMaximize.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            topPanel.Controls.Add(btnMaximize);

            Button btnClose = new Button();
            btnClose.Text = "X";
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.ForeColor = System.Drawing.Color.White;
            btnClose.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            btnClose.Size = new Size(40, 40);
            btnClose.Location = new Point(this.Width - 40, 0);
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += (s, e) => this.Close();
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = System.Drawing.Color.Red;
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            topPanel.Controls.Add(btnClose);

            this.Resize += (s, e) => {
                btnClose.Location = new Point(this.Width - 40, 0);
                btnMaximize.Location = new Point(this.Width - 80, 0);
                btnMinimize.Location = new Point(this.Width - 120, 0);
            };

            lblTitle = new Label();
            lblTitle.Text = "Will Wallpaper";
            lblTitle.Font = new Font("Segoe UI", 32, FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(100, 149, 237);
            lblTitle.BackColor = System.Drawing.Color.Transparent;
            lblTitle.Location = new Point(0, 50);
            lblTitle.Size = new Size(this.Width, 70);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.AutoSize = false;

            previewPanel = new Panel();
            previewPanel.Location = new Point(50, 140);
            previewPanel.Size = new Size(380, 240); // Preview más grande
            previewPanel.BackColor = System.Drawing.Color.Black;
            previewPanel.BorderStyle = BorderStyle.FixedSingle;

            Label lblPreview = new Label();
            lblPreview.Text = "Vista Previa del Fondo";
            lblPreview.ForeColor = System.Drawing.Color.Gray;
            lblPreview.Dock = DockStyle.Fill;
            lblPreview.TextAlign = ContentAlignment.MiddleCenter;
            previewPanel.Controls.Add(lblPreview);

            btnVideo = new Button();
            btnVideo.Text = "AGREGAR FONDO (VIDEO / GIF)";
            btnVideo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnVideo.Location = new Point(480, 140);
            btnVideo.Size = new Size(300, 90); // Botón más grande
            btnVideo.Cursor = Cursors.Hand;
            btnVideo.FlatStyle = FlatStyle.Flat;
            btnVideo.FlatAppearance.BorderSize = 0;
            btnVideo.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            btnVideo.ForeColor = System.Drawing.Color.White;
            btnVideo.Click += BtnVideo_Click;

            btnStop = new Button();
            btnStop.Text = "DETENER FONDO";
            btnStop.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnStop.Location = new Point(480, 250);
            btnStop.Size = new Size(300, 90);
            btnStop.Cursor = Cursors.Hand;
            btnStop.FlatStyle = FlatStyle.Flat;
            btnStop.FlatAppearance.BorderSize = 0;
            btnStop.BackColor = System.Drawing.Color.FromArgb(192, 57, 43);
            btnStop.ForeColor = System.Drawing.Color.White;
            btnStop.Click += BtnStop_Click;

            detailsPanel = new Panel();
            detailsPanel.Location = new Point(50, 420);
            detailsPanel.Size = new Size(730, 150); // Detalles más grandes
            detailsPanel.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            detailsPanel.BorderStyle = BorderStyle.FixedSingle;

            lblDetails = new Label();
            lblDetails.Text = "Ningún fondo activo.\n\nMemoria RAM consumida: 0 MB";
            lblDetails.Font = new Font("Segoe UI", 10);
            lblDetails.ForeColor = System.Drawing.Color.LightGray;
            lblDetails.Dock = DockStyle.Fill;
            lblDetails.TextAlign = ContentAlignment.MiddleCenter;
            detailsPanel.Controls.Add(lblDetails);

            chkStartup = new CheckBox();
            chkStartup.Text = "Iniciar automáticamente con Windows";
            chkStartup.Font = new Font("Segoe UI", 11);
            chkStartup.ForeColor = System.Drawing.Color.LightGray;
            chkStartup.BackColor = System.Drawing.Color.Transparent;
            chkStartup.Location = new Point(50, 600);
            chkStartup.Size = new Size(730, 30);
            chkStartup.Cursor = Cursors.Hand;
            chkStartup.CheckedChanged += ChkStartup_CheckedChanged;

            lblStatus = new Label();
            lblStatus.Text = "El fondo se mantendrá reproduciendose en segundo plano. Marca la casilla para que el fondo cargue al encencer tu PC siempre";
            lblStatus.Font = new Font("Segoe UI", 9);
            lblStatus.BackColor = System.Drawing.Color.Transparent;
            lblStatus.Location = new Point(0, 650);
            lblStatus.Size = new Size(this.Width, 30);
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblStatus.ForeColor = System.Drawing.Color.Gray;

            this.Controls.Add(topPanel);
            this.Controls.Add(lblTitle);
            this.Controls.Add(previewPanel);
            this.Controls.Add(btnVideo);
            this.Controls.Add(btnStop);
            this.Controls.Add(detailsPanel);
            this.Controls.Add(chkStartup);
            this.Controls.Add(lblStatus);

            CheckStartupStatus();
            UpdateStopButtonState();

            processCheckTimer = new Timer();
            processCheckTimer.Interval = 2000;
            processCheckTimer.Tick += (s, e) => UpdateStopButtonState();
            processCheckTimer.Start();

            this.Shown += (s, e) => {
                LoadInitialWallpaper();
            };
        }

        private string ResolveWallpaperPath(string saved)
        {
            if (string.IsNullOrWhiteSpace(saved)) return null;
            string baseDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string candidate = Path.IsPathRooted(saved) ? saved : Path.Combine(baseDir, saved);
            if (!File.Exists(candidate) || !IsValidVideoFile(candidate))
            {
                string fname = Path.GetFileName(saved);
                string fallback = Path.Combine(baseDir, "WallpapersGuardados", fname);
                if (File.Exists(fallback) && IsValidVideoFile(fallback)) candidate = fallback;
            }
            return (File.Exists(candidate) && IsValidVideoFile(candidate)) ? candidate : null;
        }

        private string GetConfigFilePath()
        {
            string baseDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string sysDir = Path.Combine(baseDir, "sistema");
            if (!Directory.Exists(sysDir)) Directory.CreateDirectory(sysDir);
            string sysConfig = Path.Combine(sysDir, "active_wallpaper.txt");
            string rootConfig = Path.Combine(baseDir, "active_wallpaper.txt");
            if (!File.Exists(sysConfig) && File.Exists(rootConfig))
            {
                return rootConfig;
            }
            return sysConfig;
        }

        private void LoadInitialWallpaper()
        {
            try
            {
                string baseDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                string configFile = GetConfigFilePath();
                string activeFile = null;

                if (File.Exists(configFile))
                {
                    string saved = File.ReadAllText(configFile).Trim();
                    activeFile = ResolveWallpaperPath(saved);
                }

                if (string.IsNullOrEmpty(activeFile))
                {
                    string safeFolder = Path.Combine(baseDir, "WallpapersGuardados");
                    if (Directory.Exists(safeFolder))
                    {
                        foreach (string f in Directory.GetFiles(safeFolder))
                        {
                            if (IsValidVideoFile(f))
                            {
                                activeFile = f;
                                break;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(activeFile) && File.Exists(activeFile))
                {
                    SetPreviewVideo(activeFile);
                }
            }
            catch { }
        }

        private void SetPreviewVideo(string videoPath)
        {
            if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath) || !IsValidVideoFile(videoPath)) return;
            currentPreviewPath = videoPath;

            if (elementHost == null)
            {
                elementHost = new ElementHost();
                elementHost.Dock = DockStyle.Fill;
                previewMedia = new MediaElement();
                previewMedia.LoadedBehavior = MediaState.Manual;
                previewMedia.UnloadedBehavior = MediaState.Manual;
                previewMedia.Stretch = Stretch.UniformToFill;
                previewMedia.IsMuted = true;
                
                previewMedia.MediaEnded += (s, e) => {
                    previewMedia.Position = TimeSpan.Zero;
                    previewMedia.Play();
                };

                previewMedia.MediaOpened += (s, e) => {
                    if (string.IsNullOrEmpty(currentPreviewPath) || !File.Exists(currentPreviewPath)) return;
                    FileInfo fi = new FileInfo(currentPreviewPath);
                    double mbSize = fi.Length / 1048576.0;
                    int w = previewMedia.NaturalVideoWidth;
                    int h = previewMedia.NaturalVideoHeight;
                    
                    lblDetails.Text = string.Format("Archivo activo:\n{0}\n\nTamaño: {1:F2} MB\nResolución: {2} x {3}\n¡Copia segura en WallpapersGuardados!", fi.Name, mbSize, w, h);
                };

                elementHost.Child = previewMedia;
                previewPanel.Controls.Clear();
                previewPanel.Controls.Add(elementHost);
            }
            
            previewMedia.Source = new Uri(videoPath, UriKind.Absolute);
            previewMedia.Play();
        }

        private void UpdateStopButtonState()
        {
            string currentExe = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            int currentId = Process.GetCurrentProcess().Id;
            bool isRunning = false;
            long ramUsage = 0;

            foreach (var process in Process.GetProcessesByName(currentExe))
            {
                if (process.Id != currentId)
                {
                    isRunning = true;
                    try { ramUsage += process.WorkingSet64; } catch { }
                }
            }

            btnStop.Enabled = isRunning;
            btnStop.BackColor = isRunning ? System.Drawing.Color.FromArgb(192, 57, 43) : System.Drawing.Color.FromArgb(80, 80, 80);

            if (!isRunning)
            {
                if (!lblDetails.Text.Contains("Archivo activo"))
                {
                    lblDetails.Text = "Ningún fondo activo.\n\nMemoria RAM consumida: 0 MB";
                }
            }
        }

        private void BtnVideo_Click(object sender, EventArgs e)
        {
            string baseDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string mpvPath = Path.Combine(baseDir, "sistema", "mpv.exe");
            if (!File.Exists(mpvPath)) mpvPath = Path.Combine(baseDir, "mpv.exe");

            if (!File.Exists(mpvPath))
            {
                MessageBox.Show(
                    "No se encontró el motor de video 'mpv.exe' dentro de la carpeta 'sistema'.\n\nPara que los fondos animados funcionen, debes tener el archivo 'mpv.exe' dentro de la carpeta 'sistema'.",
                    "Motor de video no encontrado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Selecciona un video o GIF";
            ofd.Filter = "Archivos de Video (*.mp4;*.mkv;*.avi;*.webm;*.wmv;*.mov)|*.mp4;*.mkv;*.avi;*.webm;*.wmv;*.mov|Archivos GIF (*.gif)|*.gif|Todos los archivos (*.*)|*.*";
            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (previewMedia != null)
                {
                    previewMedia.Stop();
                    previewMedia.Source = null;
                }

                WallpaperManager.StopCurrentWallpaper();

                string safeFolder = Path.Combine(baseDir, "WallpapersGuardados");
                if (!Directory.Exists(safeFolder))
                {
                    Directory.CreateDirectory(safeFolder);
                }

                string safePath = Path.Combine(safeFolder, Path.GetFileName(ofd.FileName));
                
                try 
                {
                    if (ofd.FileName != safePath)
                    {
                        File.Copy(ofd.FileName, safePath, true);
                    }
                } 
                catch (Exception ex)
                {
                    MessageBox.Show("Copia de seguridad falló, se usa el original. Error: " + ex.Message);
                    safePath = ofd.FileName;
                }

                try
                {
                    string configFile = GetConfigFilePath();
                    string relPath = safePath;
                    if (safePath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                    {
                        relPath = safePath.Substring(baseDir.Length).TrimStart('\\', '/');
                    }
                    File.WriteAllText(configFile, relPath);
                }
                catch { }

                SetPreviewVideo(safePath);

                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                Process.Start(exePath, string.Format("--play \"{0}\"", safePath));

                if (chkStartup.Checked)
                {
                    SetStartup(true, safePath);
                }
                
                UpdateStopButtonState();
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            WallpaperManager.StopCurrentWallpaper();
            UpdateStopButtonState();
            if (previewMedia != null)
            {
                previewMedia.Stop();
                previewMedia.Source = null;
            }
            lblDetails.Text = "Fondo detenido.\n\nMemoria RAM consumida: 0 MB";
        }

        private void ChkStartup_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkStartup.Checked)
            {
                SetStartup(false, null);
            }
            else
            {
                string configFile = GetConfigFilePath();
                if (File.Exists(configFile))
                {
                    string saved = File.ReadAllText(configFile).Trim();
                    string safePath = ResolveWallpaperPath(saved);
                    if (!string.IsNullOrEmpty(safePath))
                    {
                        SetStartup(true, safePath);
                    }
                }
            }
        }

        private void CheckStartupStatus()
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
                if (rk != null && rk.GetValue("WillWallpaper") != null)
                {
                    chkStartup.Checked = true;
                }
            }
            catch { }
        }

        private void SetStartup(bool enable, string videoPath)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (enable && videoPath != null)
                {
                    string exePath = Process.GetCurrentProcess().MainModule.FileName;
                    string baseDir = Path.GetDirectoryName(exePath);
                    string targetArg = videoPath;
                    if (videoPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                    {
                        targetArg = videoPath.Substring(baseDir.Length).TrimStart('\\', '/');
                    }
                    rk.SetValue("WillWallpaper", string.Format("\"{0}\" --play \"{1}\"", exePath, targetArg));
                    
                    try {
                        RegistryKey delayKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize");
                        if (delayKey != null) {
                            delayKey.SetValue("StartupDelayInMSec", 0, RegistryValueKind.DWord);
                        }
                    } catch { }
                }
                else if (!enable)
                {
                    rk.DeleteValue("WillWallpaper", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al configurar el inicio: " + ex.Message);
            }
        }
    }
}
