using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace Affinity_manager.Model.CRUD
{
    internal class ProcessConfigurationsRegistryManager
    {
        private const string ImageFileExecutionOptionsRegistryPath = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\";
        private const string PerfOptionsSubKeyName = "PerfOptions";
        private const string CpuPriorityClassName = "CpuPriorityClass";
        private const string IoPriorityName = "IoPriority";
        private const string PagePriorityName = "PagePriority";

        public List<ProcessConfiguration> LoadFromRegistry()
        {
            using RegistryKey? ifeoSubKey = Registry.LocalMachine.OpenSubKey(ImageFileExecutionOptionsRegistryPath)
                ?? throw new InvalidOperationException("Image File Execution Options not found!");
            string[] subKeys = ifeoSubKey.GetSubKeyNames();
            List<ProcessConfiguration> list = new(subKeys.Length);
            using ImageOptionsFiller imageOptionsFiller = new();
            foreach (string subKeyName in ifeoSubKey.GetSubKeyNames())
            {
                using RegistryKey subKey = ifeoSubKey.OpenSubKey(subKeyName)!;
                using RegistryKey? perfOptions = subKey.OpenSubKey(PerfOptionsSubKeyName);

                if (perfOptions != null)
                {
                    ProcessConfiguration processAffinity = new(subKeyName);
                    object? cpuPriorityClass = perfOptions.GetValue(CpuPriorityClassName);
                    object? ioPriority = perfOptions.GetValue(IoPriorityName);
                    object? pagePriorty = perfOptions.GetValue(PagePriorityName);

                    bool read = false;

                    processAffinity.CpuPriority = GetEnumValue(cpuPriorityClass, ProcessConfiguration.CpuPriorityDefaultValue, ref read);
                    processAffinity.IoPriority = GetEnumValue(ioPriority, ProcessConfiguration.IoPriorityDefaultValue, ref read);
                    processAffinity.MemoryPriority = GetEnumValue(pagePriorty, ProcessConfiguration.MemoryPriorityDefaultValue, ref read);

                    if (!read)
                    {
                        continue;
                    }

                    imageOptionsFiller.ReadAffinityFromRegistry(processAffinity);

                    list.Add(processAffinity);
                }
            }

            list.AddRange(imageOptionsFiller.GetAbsentItems(list).ToArray());

            return list;
        }

        public void SaveToRegistry(IEnumerable<ProcessConfiguration> items)
        {
            using RegistryKey? ifeoSubKey = Registry.LocalMachine.OpenSubKey(ImageFileExecutionOptionsRegistryPath, true)
                ?? throw new InvalidOperationException("Image File Execution Options not found!");
            using ImageOptionsFiller imageOptionsFiller = new();
            foreach (ProcessConfiguration item in items)
            {
                imageOptionsFiller.SaveToRegistry(item);
                RegistryKey? subKey = ifeoSubKey.OpenSubKey(item.Name, true);
                if (subKey == null)
                {
                    if (IfeoOptionsAreEmpty(item))
                    {
                        continue;
                    }

                    subKey = ifeoSubKey.CreateSubKey(item.Name, true)!;
                }

                try
                {
                    FillSubKey(item, subKey);

                    if (IfeoOptionsAreEmpty(item) && subKey.ValueCount == 0 && subKey.SubKeyCount == 0)
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

        private void FillSubKey(ProcessConfiguration item, RegistryKey subKey)
        {
            RegistryKey? perfOptionsKey = subKey.OpenSubKey(PerfOptionsSubKeyName, true);
            if (perfOptionsKey == null)
            {
                if (item.IoPriority == ProcessConfiguration.IoPriorityDefaultValue && item.CpuPriority == ProcessConfiguration.CpuPriorityDefaultValue)
                {
                    return;
                }

                perfOptionsKey = subKey.CreateSubKey(PerfOptionsSubKeyName, true)!;
            }

            try
            {
                AddOrRemoveValue(perfOptionsKey, CpuPriorityClassName, item.CpuPriority, ProcessConfiguration.CpuPriorityDefaultValue);
                AddOrRemoveValue(perfOptionsKey, IoPriorityName, item.IoPriority, ProcessConfiguration.IoPriorityDefaultValue);
                AddOrRemoveValue(perfOptionsKey, PagePriorityName, item.MemoryPriority, ProcessConfiguration.MemoryPriorityDefaultValue);
                bool deletePerfSubkey = perfOptionsKey.SubKeyCount == 0 && perfOptionsKey.ValueCount == 0;
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

        private void AddOrRemoveValue<T>(RegistryKey key, string valueName, T value, T defaultValue) where T : struct, Enum
        {
            bool needToRemove = value.Equals(defaultValue);

            key.SetValue(valueName, value, RegistryValueKind.DWord);
            if (needToRemove)
            {
                key.DeleteValue(valueName);
            }
        }

        private static T GetEnumValue<T>(object? value, T defaultValue, ref bool read)
            where T : struct, Enum
        {
            if (value is int enumValueInt && Enum.IsDefined(typeof(T), (uint)enumValueInt))
            {
                read = true;
                return (T)Enum.ToObject(typeof(T), (uint)enumValueInt);
            }

            return defaultValue;
        }

        private static bool IfeoOptionsAreEmpty(ProcessConfiguration config)
        {
            return config.CpuPriority == ProcessConfiguration.CpuPriorityDefaultValue
                && config.IoPriority == ProcessConfiguration.IoPriorityDefaultValue
                && config.MemoryPriority == ProcessConfiguration.MemoryPriorityDefaultValue;
        }
    }
}
