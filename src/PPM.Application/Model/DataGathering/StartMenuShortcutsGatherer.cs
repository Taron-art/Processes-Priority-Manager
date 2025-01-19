using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ShellLink;

namespace Affinity_manager.Model.DataGathering
{
    public class StartMenuShortcutsGatherer : IProcessProvider
    {
        private const string ShortcutExtension = "*.lnk";

        // We use concurrent dictionary as concurrent hashset here, the value is irrelevant.
        private readonly ConcurrentDictionary<ProcessInfo, byte> _gatheredShortcuts = new();


        public IEnumerable<ProcessInfo> GetMatchedProcesses(string searchString)
        {
            return _gatheredShortcuts.Keys.Where(info => info.Matches(searchString));
        }

        public static StartMenuShortcutsGatherer CreateAndStart()
        {
            StartMenuShortcutsGatherer instance = new();
            _ = instance.CollectAsync();
            return instance;
        }

        public Task CollectAsync()
        {
            _gatheredShortcuts.Clear();

            return Task.Run(GatherShortcuts);
        }

        private void GatherShortcuts()
        {
            GatherShortcuts(Environment.SpecialFolder.StartMenu);
            GatherShortcuts(Environment.SpecialFolder.CommonStartMenu);
        }

        private void GatherShortcuts(Environment.SpecialFolder startMenu)
        {
            string startMenuPath = Environment.GetFolderPath(startMenu);
            IEnumerable<string> shortcuts = Directory.EnumerateFiles(startMenuPath, ShortcutExtension, SearchOption.AllDirectories);

            foreach (string shortcut in shortcuts)
            {
                ProcessInfo? processInfo = CreateProcessInfoFromShortcut(shortcut);
                if (processInfo != null)
                {
                    _gatheredShortcuts[processInfo] = 0;
                }
            }
        }

        private ProcessInfo? CreateProcessInfoFromShortcut(string shortcutPath)
        {
            Shortcut shortcut;
            try
            {
                shortcut = Shortcut.ReadFromFile(shortcutPath);
            }
            catch (ArgumentException)
            {
                // Invalid shortcut, just skip it.
                return null;
            }

            string? targetPath = shortcut.GetExeTargetFullPath();

            if (string.IsNullOrEmpty(targetPath) || !File.Exists(targetPath))
            {
                return null;
            }

            ProcessInfo processInfo = new(Path.GetFileName(targetPath));

            string? iconPath = shortcut.GetIconPath();
            processInfo.UpdateWithFriendlyNameAndModulePath(Path.GetFileNameWithoutExtension(shortcutPath), iconPath);

            return processInfo;
        }
    }
}
