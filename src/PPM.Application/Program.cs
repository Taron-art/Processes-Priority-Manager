using System;
using System.Collections.Generic;
using Affinity_manager.Hosting;
using Affinity_manager.Model;
using Affinity_manager.Model.CRUD;
using Affinity_manager.Model.DataGathering;
using Affinity_manager.ViewModels;
using Affinity_manager.ViewWrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

namespace Affinity_manager
{
    public static class Program
    {
        [System.Runtime.InteropServices.DllImport("Microsoft.ui.xaml.dll")]
        private static extern void XamlCheckProcessRequirements();

        [STAThread]
        static void Main(string[] args)
        {
            XamlCheckProcessRequirements();

            HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings { Args = args });

            AddDIServices(builder.Services);
            builder.Build().Run();
        }

        private static void AddDIServices(IServiceCollection services)
        {
            services.AddSingleton<App>();
            services.AddSingleton<Application>(static provider => provider.GetService<App>()!);
            services.AddTransient<MainWindow>();
            services.AddTransient<IMainPageViewModel, MainPageViewModel>();
            services.AddTransient<IProcessConfigurationsRepository, ProcessConfigurationsRepository>();
            services.AddTransient<IProcessConfigurationViewFactory, ProcessConfigurationViewFactory>();
            services.AddTransient<IOptionsProvider, OptionsProvider>();
            services.AddTransient<IEqualityComparer<ProcessConfigurationView>, ProcessConfigurationViewEqualityComparer>();
            services.AddSingleton(_ => ProcessesMonitor.CreateAndStart()); // Monitors running processes, must be one.
            services.AddSingleton<IApplicationIconsLoader, ApplicationIconsLoader>(); // must be one because it uses shared resource - access to file system that may crash.
            services.AddTransient(_ => StartMenuShortcutsGatherer.CreateAndStart());
            services.AddTransient<IProcessConfigurationApplier, ProcessConfigurationApplier>();
            services.AddTransient<IAutocompleteProvider, AutocompleteView>((provider) =>
            {
                AutocompleteView view = new(provider.GetRequiredService<IApplicationIconsLoader>());
                view.AddProcessProvider(provider.GetRequiredService<ProcessesMonitor>());
                view.AddProcessProvider(provider.GetRequiredService<StartMenuShortcutsGatherer>());
                return view;
            });
            services.AddHostedService<WinUIHostingService>();
        }
    }
}
