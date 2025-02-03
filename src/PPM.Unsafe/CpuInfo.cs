using Vanara.Extensions;
using static Vanara.PInvoke.Kernel32;

namespace PPM.Unsafe
{
    public class CpuInfo
    {
        public static unsafe uint GetLogicalProcessorsCount()
        {
            Vanara.PInvoke.Win32Error result = GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationGroup, out SafeSYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX_List info);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Failed to get logical processor information.", result.GetException());
            }

            uint cpuCount = 0;

            for (int i = 0; i < info.Count; i++)
            {
                SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX cpuInfo = ((IntPtr)info[i]).ToStructure<SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX>();
                for (int j = 0; j < cpuInfo.Group.ActiveGroupCount; j++)
                {
                    cpuCount += cpuInfo.Group.GroupInfo[j].ActiveProcessorCount;
                }
            }
            info.Dispose();
            return cpuCount;
        }

        public static unsafe CoreInfo[] GetCoreInfos()
        {
            Vanara.PInvoke.Win32Error result = GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationAll, out SafeSYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX_List info);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Failed to get logical processor information.", result.GetException());
            }

            int numberOfCpuCores = 0;
            int itemsCount = info.Count;
            for (int i = 0; i < itemsCount; i++)
            {
                SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX cpuInfo = ((IntPtr)info[i]).ToStructure<SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX>();
                if (cpuInfo.Relationship == LOGICAL_PROCESSOR_RELATIONSHIP.RelationNumaNode)
                {
                    numberOfCpuCores += cpuInfo.NumaNode.GroupMask.AffinitizedProcessors.Count();
                }
            }

            CoreInfo[] coreInfos = Enumerable.Range(0, numberOfCpuCores).Select(index => new CoreInfo { Id = (uint)index }).ToArray();

            int coreGroupIndex = 0;
            uint[] cacheGroupIndexByLevel = new uint[3];

            for (int i = 0; i < itemsCount; i++)
            {
                SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX cpuInfo = ((IntPtr)info[i]).ToStructure<SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX>();

                if (cpuInfo.Relationship == LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore)
                {
                    PhysicalCoreGroup physicalCoreGroup = new() { Id = (uint)coreGroupIndex++ };
                    PerformanceCoreGroup performanceCoreGroup = new() { Id = cpuInfo.Processor.EfficiencyClass };
                    foreach (GROUP_AFFINITY group in cpuInfo.Processor.GroupMask)
                    {
                        foreach (uint cpuIndex in group.AffinitizedProcessors)
                        {
                            coreInfos[(int)cpuIndex].AddAssociatedGroup(performanceCoreGroup);
                            coreInfos[(int)cpuIndex].AddAssociatedGroup(physicalCoreGroup);
                        }
                    }
                }
                else if (cpuInfo.Relationship == LOGICAL_PROCESSOR_RELATIONSHIP.RelationCache)
                {
                    if (cpuInfo.Cache.Level > 3)
                    {
                        // May be there will be Level 4 cache in the future and we will crash...
                        continue;
                    }

                    CacheCoreGroup group = new()
                    {
                        Id = cacheGroupIndexByLevel[cpuInfo.Cache.Level - 1]++,
                        CacheSizeInB = cpuInfo.Cache.CacheSize,
                        Level = cpuInfo.Cache.Level
                    };

                    foreach (uint cpuIndex in cpuInfo.Cache.GroupMask.AffinitizedProcessors)
                    {
                        coreInfos[(int)cpuIndex].AddAssociatedGroup(group);
                    }
                }
            }

            info.Dispose();
            Array.ForEach(coreInfos, coreInfo => coreInfo.Seal());
            return coreInfos;
        }
    }
}
