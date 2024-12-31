using System;
using System.Diagnostics;
using System.Linq;
using Affinity_manager.Model.CRUD;
using Affinity_manager.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vanara.PInvoke;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Affinity_manager
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            // Add some pre-crash window here.
            Trace.WriteLine(e.ExceptionObject);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            if (Environment.GetCommandLineArgs().Contains("--clear"))
            {
                ProcessConfigurationsRepository cleaner = new();
                cleaner.Clean();
                Environment.Exit(0);
            }



            m_window = new MainWindow();
            m_window.AppWindow.SetIcon("Assets\\tune.ico");
            double scale = User32.GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(m_window)) / 96.0;
            m_window.AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(630 * scale), (int)(500 * scale)));
            Frame rootFrame = new();
            rootFrame.Navigate(typeof(MainPage), args.Arguments);
            m_window.Content = rootFrame;
            m_window.Activate();
        }

        private Window? m_window;
    }
}
