using System.Collections.Generic;
using Affinity_manager.Model;
using PPM.Unsafe;

namespace Affinity_manager.ViewWrappers
{
    public interface IOptionsProvider
    {
        IReadOnlyList<EnumViewWrapper<CpuPriorityClass>> CpuPriorities { get; }
        IReadOnlyList<EnumViewWrapper<IoPriority>> IoPriorities { get; }
        IReadOnlyList<EnumViewWrapper<PagePriority>> MemoryPriorities { get; }

        uint NumberOfLogicalCpus { get; }
        IReadOnlyList<CoreInfo> ProcessorCoresInfo { get; }
    }
}
