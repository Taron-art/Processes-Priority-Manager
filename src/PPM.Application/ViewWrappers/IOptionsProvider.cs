using System.Collections.Generic;
using Affinity_manager.Model;

namespace Affinity_manager.ViewWrappers
{
    public interface IOptionsProvider
    {
        IReadOnlyList<EnumViewWrapper<CpuPriorityClass>> CpuPriorities { get; }
        IReadOnlyList<EnumViewWrapper<IoPriority>> IoPriorities { get; }
        IReadOnlyList<EnumViewWrapper<PagePriority>> MemoryPriorities { get; }

        uint NumberOfLogicalCpus { get; }
    }
}
