using NAudio.Wave;
using Spectrogram;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            PMW_lib.PMWFingerprinting.DisposeModelService();
        }

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length > 0)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                if (e.Args.Length > 0)
                {
                    PMW_lib.PMWFingerprinting.InitilizeModelService();
                    ConsoleModel.ProcessArgs(e.Args);

                }
                Current.Shutdown();
            }
            else
            {
                FolderView folderView = new ();
                folderView.Show();
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            PMW_lib.PMWFingerprinting.InitilizeModelService();
        }

    }
}
