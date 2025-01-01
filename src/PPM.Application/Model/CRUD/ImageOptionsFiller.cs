using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace Affinity_manager.Model.CRUD
{
    internal class ImageOptionsFiller : IDisposable
    {
        public const string ImageOptionsRegistryPath = "SOFTWARE\\Processes Priority Manager\\Image Options\\";
        public const string AffinityValueName = "Affinity";

        private RegistryKey? _imageOptionsKey;

        public void ReadAffinityFromRegistry(ProcessConfiguration item)
        {
            using RegistryKey? processKey = GetImageOptions(false)?.OpenSubKey(item.Name);
            if (processKey == null)
            {
                item.CpuAffinityMask = ProcessConfiguration.AffinityDefaultValue;
                return;
            }

            item.CpuAffinityMask = ReadAffinityValue(processKey);
        }

        public void SaveToRegistry(ProcessConfiguration item)
        {
            bool needToSave = item.CpuAffinityMask != ProcessConfiguration.AffinityDefaultValue;
            RegistryKey? imageOptionsKey = GetImageOptions(needToSave);
            if (imageOptionsKey == null && needToSave)
            {
                throw new InvalidOperationException("Image Options not found!");
            }
            else if (imageOptionsKey == null)
            {
                return;
            }

            if (needToSave)
            {
                using RegistryKey processKey = imageOptionsKey.OpenSubKey(item.Name, true) ?? imageOptionsKey.CreateSubKey(item.Name);
                processKey.SetValue(AffinityValueName, unchecked((long)item.CpuAffinityMask), RegistryValueKind.QWord);
            }
            else
            {
                using RegistryKey? processKey = imageOptionsKey.OpenSubKey(item.Name, true);
                if (processKey == null)
                {
                    return;
                }

                processKey.DeleteValue(AffinityValueName);
                if (processKey.SubKeyCount == 0 && processKey.ValueCount == 0)
                {
                    processKey.DeleteSubKey(string.Empty);
                }
            }
        }

        public IEnumerable<ProcessConfiguration> GetAbsentItems(IReadOnlyList<ProcessConfiguration> items)
        {
            RegistryKey? imageOptionsKey = GetImageOptions(false);
            if (imageOptionsKey == null)
            {
                yield break;
            }

            foreach (string key in imageOptionsKey.GetSubKeyNames())
            {
                if (!items.Any(item => string.Equals(item.Name, key, StringComparison.OrdinalIgnoreCase)))
                {
                    using RegistryKey processKey = imageOptionsKey.OpenSubKey(key)!;
                    yield return new ProcessConfiguration(key)
                    {
                        CpuAffinityMask = ReadAffinityValue(processKey)
                    };
                }
            }
        }

        private RegistryKey? GetImageOptions(bool createIfNotExist)
        {
            return _imageOptionsKey ??= Registry.LocalMachine.OpenSubKey(ImageOptionsRegistryPath, true)
                ?? (createIfNotExist ? Registry.LocalMachine.CreateSubKey(ImageOptionsRegistryPath) : null);
        }

        private static ulong ReadAffinityValue(RegistryKey processKey)
        {
            if (processKey.GetValue(AffinityValueName) == null || processKey.GetValueKind(AffinityValueName) != RegistryValueKind.QWord)
            {
                return ProcessConfiguration.AffinityDefaultValue;
            }

            return unchecked((ulong)(long)processKey.GetValue(AffinityValueName)!);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _imageOptionsKey?.Dispose();
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
