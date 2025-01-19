using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Affinity_manager.Model.DataGathering;
using NUnit.Framework;

namespace PPM.Application.IntegrationTests.Model.DataGathering
{
    [TestFixture]
    public class ProcessesMonitorTests
    {
        private readonly string _windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        [Test]
        [Retry(5)] // Since process list can change, we would like to try to run at lease a few times.
        public async Task ProcessesMonitor_ReturnsRunningProcessesWithMainModuleAndWindowTitle()
        {
            // Arrange
            using ProcessesMonitor monitor = new();

            Task monitoringStart = monitor.StartMonitoringAsync();

            // Act
            var realProcesses = Process.GetProcesses()
                .Where(p =>
                {
                    try
                    {
                        ProcessModule? module = p.MainModule;
                        return module != null && !string.IsNullOrEmpty(p.MainWindowTitle) && !(module.FileName.StartsWith(_windowsPath, StringComparison.OrdinalIgnoreCase));
                    }
                    catch
                    {
                        return false;
                    }
                })
                .Select(p => new { p.MainModule!.ModuleName, p.MainModule!.FileName, p.MainWindowTitle })
                .ToList();

            await monitoringStart; // Give some time for the monitor to gather processes
            System.Collections.Generic.List<ProcessInfo> monitoredProcesses = monitor.GetMatchedProcesses(string.Empty).ToList();

            if (realProcesses.Count == 0)
            {
                Assert.Ignore("There is no applications with named windows running, cannot verify");
            }

            // Assert
            Assert.That(monitoredProcesses, Is.Not.Empty);
            foreach (var realProcess in realProcesses)
            {
                ProcessInfo? monitoredProcess = monitoredProcesses.FirstOrDefault(p => p.MainModuleName == realProcess.ModuleName);
                Assert.That(monitoredProcess, Is.Not.Null);
                Assert.That(monitoredProcess.Source, Is.EqualTo(Source.RunningTasks));
                Assert.That(monitoredProcess.FriendlyName, Is.EqualTo(realProcess.MainWindowTitle));
                Assert.That(monitoredProcess.ModuleFullPath, Is.EqualTo(realProcess.FileName));
            }
        }
    }
}
