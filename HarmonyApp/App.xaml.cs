using HarmonyApp.AudioProcessing;
using HarmonyApp.Models;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace HarmonyApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Fingerprinting.InitilizeModelService();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Fingerprinting.DisposeModelService();
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        protected override async void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                await ConsoleModel.ProcessArgs(e.Args);
                Fingerprinting.DisposeModelService();
                Current.Shutdown();
            }
            else
            {
                var handle = GetConsoleWindow();
                ShowWindow(handle, SW_HIDE);

                base.OnStartup(e);
                Views.FolderView folderView = new();
                folderView.Show();
            }
        }
    }
}
