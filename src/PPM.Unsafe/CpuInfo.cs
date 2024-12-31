using Vanara.Extensions;
using static Vanara.PInvoke.Kernel32;

namespace PPM.Unsafe
{
    public static class CpuInfo
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
                var cpuInfo = ((IntPtr)info[i]).ToStructure<SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX>();
                for (int j = 0; j < cpuInfo.Group.ActiveGroupCount; j++)
                {
                    cpuCount += cpuInfo.Group.GroupInfo[j].ActiveProcessorCount;
                }
            }
            info.Dispose();
            return cpuCount;
        }
    }
}
