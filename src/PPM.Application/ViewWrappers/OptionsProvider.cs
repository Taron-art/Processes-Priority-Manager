using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Affinity_manager.Model;
using PPM.Unsafe;

namespace Affinity_manager.ViewWrappers
{
    public class OptionsProvider : IOptionsProvider
    {
        private IReadOnlyList<EnumViewWrapper<IoPriority>>? _ioPriorities;
        private IReadOnlyList<EnumViewWrapper<CpuPriorityClass>>? _cpuPriorities;
        private IReadOnlyList<EnumViewWrapper<PagePriority>>? _memoryPriorities;
        private IReadOnlyList<CoreInfo>? _processorCoresInformation;

        public IReadOnlyList<EnumViewWrapper<CpuPriorityClass>> CpuPriorities
        {
            get
            {
                return _cpuPriorities ??= CreateViews<CpuPriorityClass>();
            }
        }

        public IReadOnlyList<EnumViewWrapper<IoPriority>> IoPriorities
        {
            get
            {
                return _ioPriorities ??= CreateViews<IoPriority>();
            }
        }

        public IReadOnlyList<EnumViewWrapper<PagePriority>> MemoryPriorities
        {
            get
            {
                return _memoryPriorities ??= CreateViews<PagePriority>();
            }
        }

        public uint NumberOfLogicalCpus
        {
            get
            {
                return (uint)ProcessorCoresInfo.Count;
            }
        }

        public IReadOnlyList<CoreInfo> ProcessorCoresInfo
        {
            get
            {
                return (_processorCoresInformation ??= _processorCoresInformation = CpuInfo.GetCoreInfos());
            }
        }

        private static List<EnumViewWrapper<T>> CreateViews<T>()
            where T : struct, Enum
        {
            return Enum.GetValues<T>().OrderBy(GetOrder<T>).Select((enumValue) => new EnumViewWrapper<T>(enumValue)).ToList();
        }

        private static uint? GetOrder<T>(T enumValue) where T : struct, Enum
        {
            return (uint?)(enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                ?.GetOrder()) ?? Convert.ToUInt32(enumValue);
        }
    }
}
