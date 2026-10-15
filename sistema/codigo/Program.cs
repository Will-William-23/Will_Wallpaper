using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

[assembly: AssemblyTitle("Will Wallpaper")]
[assembly: AssemblyDescription("Reproductor de fondos de pantalla animados")]
[assembly: AssemblyProduct("Will Wallpaper")]
[assembly: AssemblyCopyright("Copyright © 2026")]

namespace WillWallpaper
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length >= 2 && args[0] == "--play")
            {
                WallpaperManager.RunBackgroundWallpaper(args[1]);
            }
            else if (args.Length == 1)
            {
                WallpaperManager.RunBackgroundWallpaper(args[0]);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ConfigForm());
            }
        }
    }
}
