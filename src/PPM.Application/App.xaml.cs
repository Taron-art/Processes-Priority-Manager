using System;
using System.Diagnostics;
using System.Linq;
using Affinity_manager.Model.CRUD;
using Affinity_manager.Pages;
using Affinity_manager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vanara.PInvoke;

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
        public App(IServiceProvider serviceProvider)
        {
            this.InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            _serviceProvider = serviceProvider;
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            // Add some pre-crash window here.
            Trace.WriteLine(e.ExceptionObject);
        }

        private const double HundredPercentWindowsDPI = 96;

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (Environment.GetCommandLineArgs().Contains("--clear"))
            {
                ProcessConfigurationsRepository cleaner = new();
                cleaner.CleanWithoutServiceRestart();
                Environment.Exit(0);
            }

            m_window = _serviceProvider.GetRequiredService<MainWindow>();
            m_window.AppWindow.SetIcon("Assets\\tune.ico");
            double scale = User32.GetDpiForWindow(WinRT.Interop.WindowNative.GetWindowHandle(m_window)) / HundredPercentWindowsDPI;
            m_window.AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(630 * scale), (int)(500 * scale)));
            Frame rootFrame = new();
            rootFrame.Navigate(typeof(MainPage), _serviceProvider.GetRequiredService<IMainPageViewModel>());
            m_window.Content = rootFrame;
            m_window.Activate();
        }

        private Window? m_window;
        private readonly IServiceProvider _serviceProvider;
    }
}
