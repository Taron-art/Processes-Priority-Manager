using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Affinity_manager.Exceptions;

namespace Affinity_manager.Model.CRUD
{
    internal class ProcessConfigurationsRepository : IProcessConfigurationsRepository
    {
        private const string ServiceName = "PPM_Service";
        private readonly ProcessConfigurationsRegistryManager _processAffinitiesManager = new();

        public List<ProcessConfiguration> Get()
        {
            return _processAffinitiesManager.LoadFromRegistry();
        }

        public void Save(IEnumerable<ProcessConfiguration> items)
        {
            Save(items, false);
        }

        public async Task SaveAndRestartServiceAsync(IEnumerable<ProcessConfiguration> items, Func<Task>? readyToGetCallback = null)
        {
            ProcessConfiguration[] itemsArray = items.ToArray();
            ThrowIfHaveInvalidNonEmptyItems(itemsArray);

            await Task.Run(() => _processAffinitiesManager.SaveToRegistry(items));

            // We want UI to be responsive, so we restart the service in the background while UI performing a callback.
            Task? task = readyToGetCallback?.Invoke();

            await Task.Run(RestartService);
            if (task != null)
            {
                await task;
            }
        }

        public void CleanWithoutServiceRestart()
        {
            List<ProcessConfiguration> processAffinities = Get();
            foreach (ProcessConfiguration processAffinity in processAffinities)
            {
                processAffinity.Reset();
            }

            Save(processAffinities, false);
        }

        private void Save(IEnumerable<ProcessConfiguration> items, bool restartService)
        {
            ProcessConfiguration[] itemsArray = items.ToArray();
            ThrowIfHaveInvalidNonEmptyItems(itemsArray);

            _processAffinitiesManager.SaveToRegistry(items);
            if (restartService)
            {
                RestartService();
            }
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

        private static void ThrowIfHaveInvalidNonEmptyItems(ProcessConfiguration[] itemsArray)
        {
            if (itemsArray.Any((item) => item.HasErrors && !item.IsEmpty)) // We are passing empty values since they won't introduce invalid items.
            {
                throw new ValidationException("At least one of the items is invalid.");
            }
        }
    }
}
