using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Affinity_manager.Model;

namespace Affinity_manager.ViewWrappers
{
    public class OptionsProvider
    {
        private IReadOnlyList<EnumViewWrapper<IoPriority>>? _ioPriorities;
        private IReadOnlyList<EnumViewWrapper<CpuPriorityClass>>? _cpuPriorities;

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

        private List<EnumViewWrapper<T>> CreateViews<T>()
            where T : struct, Enum
        {
            return Enum.GetValues<T>().OrderBy(enumValue => GetOrder<T>(enumValue)).Select((enumValue) => new EnumViewWrapper<T>(enumValue)).ToList();
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
