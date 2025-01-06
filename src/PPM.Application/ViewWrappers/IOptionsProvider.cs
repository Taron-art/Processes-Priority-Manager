using System.Collections.Generic;
using Affinity_manager.Model;

namespace Affinity_manager.ViewWrappers
{
    public interface IOptionsProvider
    {
        IReadOnlyList<EnumViewWrapper<CpuPriorityClass>> CpuPriorities { get; }
        IReadOnlyList<EnumViewWrapper<IoPriority>> IoPriorities { get; }
        uint NumberOfLogicalCpus { get; }
    }
}