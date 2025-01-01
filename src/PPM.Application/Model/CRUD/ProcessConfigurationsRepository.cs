using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading.Tasks;
using Affinity_manager.Exceptions;

namespace Affinity_manager.Model.CRUD
{
    internal class ProcessConfigurationsRepository
    {
        private const string ServiceName = "PPM_Service";
        private readonly ProcessConfigurationsRegistryManager _processAffinitiesManager = new();

        public List<ProcessConfiguration> Get()
        {
            return _processAffinitiesManager.LoadFromRegistry();
        }

        public void Save(IEnumerable<ProcessConfiguration> items)
        {
            _processAffinitiesManager.SaveToRegistry(items);
            RestartService();
        }

        public async Task SaveAsync(IEnumerable<ProcessConfiguration> items, Func<Task>? readyToGetCallback = null)
        {
            await Task.Run(() => _processAffinitiesManager.SaveToRegistry(items));

            // We want UI to be responsive, so we restart the service in the background while UI performing a callback.
            Task? task = readyToGetCallback?.Invoke();

            await Task.Run(RestartService);
            if (task != null)
            {
                await task;
            }
        }

        public void Clean()
        {
            List<ProcessConfiguration> processAffinities = Get();
            foreach (ProcessConfiguration processAffinity in processAffinities)
            {
                processAffinity.Reset();
            }

            Save(processAffinities);
        }

        private void RestartService()
        {
            using ServiceController serviceController = new(ServiceName);

            try
            {
                if (serviceController.Status != ServiceControllerStatus.Stopped && serviceController.Status != ServiceControllerStatus.StopPending)
                {
                    serviceController.Stop();
                    serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                }

                serviceController.Start();
            }
            catch (InvalidOperationException e)
            {
                ServiceNotInstalledException.ThrowFromInvalidOperationException(e, ServiceName);
            }
        }
    }
}
