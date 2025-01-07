using System.Collections.Generic;
using System.Threading.Tasks;
using Affinity_manager.Hosting;
using Affinity_manager.Model.CRUD;
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
            services.AddHostedService<WinUIHostingService>();
        }
    }
}
