using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Affinity_manager.Model.DataGathering
{
    public sealed class ProcessesMonitor : IDisposable, IProcessProvider
    {
        private const int timeoutBetweenSearches = 10;

        // We use concurrent dictionary as concurrent hashset here, the value is irrelevant.
        private readonly ConcurrentDictionary<ProcessInfo, byte> _historicProcesses = new();
        private IReadOnlyList<ProcessInfo>? _activeProcesses;

        private readonly Timer _timer;
        private readonly string _windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        private readonly Dictionary<string, byte> _processesToIgnore = new(StringComparer.OrdinalIgnoreCase);

        public ProcessesMonitor()
        {
            _timer = new Timer(TimeSpan.FromSeconds(timeoutBetweenSearches));
            _timer.Elapsed += Timer_Elapsed;
        }

        public static ProcessesMonitor CreateAndStart()
        {
            var processMonitor = new ProcessesMonitor();
            _ = processMonitor.StartMonitoringAsync();
            return processMonitor;
        }

        public bool IsWorking { get => _timer.Enabled; }

        public async Task StartMonitoringAsync()
        {
            await Task.Run(RefreshProcesses);
            _timer.Enabled = true;
        }

        public IEnumerable<ProcessInfo> GetMatchedProcesses(string searchString)
        {
            return _historicProcesses.Keys.Where(info => info.Matches(searchString));
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            RefreshProcesses();
        }

        private void RefreshProcesses()
        {
            Process[] processes = Process.GetProcesses();

            List<ProcessInfo> activeProcesses = new(processes.Length);
            foreach (Process process in processes.Where(process => !_processesToIgnore.ContainsKey(process.ProcessName)))
            {
                ProcessInfo? processInfo = null;
                try
                {
                    ProcessModule? mainModule = process.MainModule;
                    if (mainModule != null && !(mainModule.FileName.StartsWith(_windowsPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        processInfo = new(mainModule.ModuleName, Source.RunningTasks);
                        processInfo.UpdateWithFriendlyNameAndModulePath(process.MainWindowTitle, mainModule.FileName);
                    }
                }
                catch (Win32Exception)
                {
                    // System process just skip it.
                    _processesToIgnore[process.ProcessName] = 0;
                }
                catch (InvalidOperationException)
                {
                    // Exited process, ignore.
                }
                finally
                {
                    process.Dispose();
                }

                if (processInfo is not null)
                {
                    _historicProcesses.AddOrUpdate(processInfo, 0, (info, _) =>
                    {
                        info.UpdateWithFriendlyNameAndModulePath(processInfo.FriendlyName, processInfo.ModuleFullPath);
                        return 0;
                    });

                    activeProcesses.Add(processInfo);
                }
            }

            _activeProcesses = activeProcesses;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
