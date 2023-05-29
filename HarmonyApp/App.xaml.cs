using HarmonyApp.AudioProcessing;
using HarmonyApp.Models;
using System.Runtime.InteropServices;
using System.Windows;

namespace HarmonyApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Fingerprinting.DisposeModelService();
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
                    Fingerprinting.InitilizeModelService();
                    ConsoleModel.ProcessArgs(e.Args);
                }
                Current.Shutdown();
            }
            else
            {
                Views.FolderView folderView = new ();
                folderView.Show();
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Fingerprinting.InitilizeModelService();
        }

    }
}
