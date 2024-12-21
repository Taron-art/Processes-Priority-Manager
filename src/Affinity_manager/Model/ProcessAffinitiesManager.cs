using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Affinity_manager.Model
{
    internal class ProcessAffinitiesManager
    {
        private const string ImageFileExecutionOptionsRegistryPath = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\";
        private const string PerfOptionsSubKeyName = "PerfOptions";
        private const string CpuPriorityClassName = "CpuPriorityClass";
        private const string IoPriorityName = "IoPriority";
        private const string DebuggerName = "Debugger";
        private readonly DebuggerKeyManager _debuggerKeyManager = new();

        public IEnumerable<ProcessAffinity> LoadFromRegistry()
        {
            using RegistryKey? ifeoSubKey = Registry.LocalMachine.OpenSubKey(ImageFileExecutionOptionsRegistryPath);
            if (ifeoSubKey == null)
            {
                throw new InvalidOperationException("Image File Execution Options not found!");
            }

            string[] subKeys = ifeoSubKey.GetSubKeyNames();
            List<ProcessAffinity> list = new(subKeys.Length);
            foreach (string subKeyName in ifeoSubKey.GetSubKeyNames())
            {
                using RegistryKey subKey = ifeoSubKey.OpenSubKey(subKeyName)!;
                using RegistryKey? perfOptions = subKey.OpenSubKey(PerfOptionsSubKeyName);
                var debuggerValue = subKey.GetValue(DebuggerName, null);

                uint affinity = uint.MaxValue;
                if (debuggerValue is string affinitySetter && _debuggerKeyManager.TryParseDebugLine(affinitySetter, out affinity) || perfOptions != null)
                {
                    ProcessAffinity processAffinity = new(subKeyName)
                    {
                        CpuAffinityMask = affinity
                    };

                    if (perfOptions != null)
                    {
                        object? cpuPriorityClass = perfOptions.GetValue(CpuPriorityClassName);
                        object? ioPriority = perfOptions.GetValue(IoPriorityName);

                        processAffinity.CpuPriority = GetEnumValue(cpuPriorityClass, CpuPriorityClass.Normal);
                        processAffinity.IoPriority = GetEnumValue(ioPriority, IoPriority.Normal);
                    }

                    yield return processAffinity;
                }
            }
        }

        public void SaveToRegistry(IEnumerable<ProcessAffinity> items)
        {
            using RegistryKey? ifeoSubKey = Registry.LocalMachine.OpenSubKey(ImageFileExecutionOptionsRegistryPath, true);
            if (ifeoSubKey == null)
            {
                throw new InvalidOperationException("Image File Execution Options not found!");
            }

            foreach (ProcessAffinity item in items)
            {
                RegistryKey? subKey = ifeoSubKey.OpenSubKey(item.Name, true);
                if (subKey == null)
                {
                    if (item.IsEmpty)
                    {
                        continue;
                    }

                    subKey = ifeoSubKey.CreateSubKey(item.Name, true)!;
                }

                try
                {
                    FillSubKey(item, subKey);

                    if (item.IsEmpty && subKey.ValueCount == 0 && subKey.SubKeyCount == 0)
                    {
                        subKey.DeleteSubKey(string.Empty);
                    }
                }
                finally
                {
                    subKey.Dispose();
                }
            }
        }

        private void FillSubKey(ProcessAffinity item, RegistryKey subKey)
        {
            AddOrRemoveDebuggerValue(subKey, item.CpuAffinityMask);

            RegistryKey? perfOptionsKey = subKey.OpenSubKey(PerfOptionsSubKeyName, true);
            if (perfOptionsKey == null)
            {
                if (item.IoPriority == ProcessAffinity.IoPriorityDefaultValue && item.CpuPriority == ProcessAffinity.CpuPriorityDefaultValue)
                {
                    return;
                }

                perfOptionsKey = subKey.CreateSubKey(PerfOptionsSubKeyName, true)!;
            }

            bool deletePerfSubkey = false;

            try
            {
                AddOrRemoveValue(perfOptionsKey, CpuPriorityClassName, item.CpuPriority, ProcessAffinity.CpuPriorityDefaultValue);
                AddOrRemoveValue(perfOptionsKey, IoPriorityName, item.IoPriority, ProcessAffinity.IoPriorityDefaultValue);
                deletePerfSubkey = perfOptionsKey.SubKeyCount == 0 && perfOptionsKey.ValueCount == 0;
                if (deletePerfSubkey)
                {
                    perfOptionsKey.DeleteSubKey(string.Empty);
                }

            }
            finally
            {
                perfOptionsKey.Dispose();
            }
        }

        private void AddOrRemoveDebuggerValue(RegistryKey subKey, uint cpuAffinityMask)
        {
            var debuggerLine = _debuggerKeyManager.CreateDebuggerLine(cpuAffinityMask);
            if (string.IsNullOrEmpty(debuggerLine))
            {
                subKey.DeleteValue(DebuggerName, false);
                return;
            }
            else
            {
                subKey.SetValue(DebuggerName, debuggerLine);
            }
        }

        private void AddOrRemoveValue<T>(RegistryKey key, string valueName, T value, T defaultValue) where T : struct, Enum
        {
            bool needToRemove = value.Equals(defaultValue);

            key.SetValue(valueName, value, RegistryValueKind.DWord);
            if (needToRemove)
            {
                key.DeleteValue(valueName);
            }
        }

        private static T GetEnumValue<T>(object? value, T defaultValue)
            where T : struct, Enum
        {
            if (value is int enumValueInt && Enum.IsDefined(typeof(T), (uint)enumValueInt))
            {
                return (T)Enum.ToObject(typeof(T), (uint)enumValueInt);
            }

            return defaultValue;
        }
    }
}
