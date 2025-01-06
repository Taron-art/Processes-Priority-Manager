using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Affinity_manager.Hosting
{
    public class WinUIHostingService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public WinUIHostingService(IServiceProvider serviceProvider, IHostApplicationLifetime applicationLifetime)
        {
            _serviceProvider = serviceProvider;
            _applicationLifetime = applicationLifetime;
        }

        private DispatcherQueue? _dispatcherQueue;
        private Application? _app;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Thread uiThread = new(() =>
            {
                WinRT.ComWrappersSupport.InitializeComWrappers();
                Application.Start(_ =>
                {
                    _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                    DispatcherQueueSynchronizationContext context = new(_dispatcherQueue);
                    SynchronizationContext.SetSynchronizationContext(context);

                    _app = (Application?)_serviceProvider.GetService(typeof(Application));
                });

                _dispatcherQueue = null;

                if (!cancellationToken.IsCancellationRequested)
                {
                    _applicationLifetime.StopApplication();
                }
            });

            uiThread.Name = "WinUI Thread";
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            TaskCompletionSource completion = new();
            bool result = !(_dispatcherQueue?.TryEnqueue(() =>
            {
                _app?.Exit();
                completion.SetResult();
            })) ?? false;


            return result ? completion.Task : Task.CompletedTask;
        }
    }
}
