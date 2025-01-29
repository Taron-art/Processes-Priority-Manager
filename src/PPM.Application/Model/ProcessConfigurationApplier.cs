using System;
using System.Diagnostics;
using System.IO;
using PPM.Unsafe;

namespace Affinity_manager.Model
{
    public class ProcessConfigurationApplier : IProcessConfigurationApplier
    {
        public bool ApplyIfPresent(byte processorCount, ProcessConfiguration configuration)
        {
            bool applied = false;
            nint processorMask = CalculateProcessorMask(processorCount);
            foreach (Process process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(configuration.Name)))
            {
                try
                {
                    UpdateProcess(process, configuration, processorMask);
                    applied = true;
                }
                catch (InvalidOperationException)
                {
                    // Process was already terminated, ignore.
                }
            }
            return applied;
        }

        private static void UpdateProcess(Process process, ProcessConfiguration configuration, nint processorMask)
        {
            process.PriorityClass = Map(configuration.CpuPriority);
            if (processorMask > 0)
            {
                unchecked
                {
                    process.ProcessorAffinity = ((nint)configuration.CpuAffinityMask) & processorMask;
                }
            }

            process.SetIoPriority((IoPriorityHint)configuration.IoPriority);
            process.SetMemoryPriority((PagePriorityInformation)configuration.MemoryPriority);
        }

        private static nint CalculateProcessorMask(byte processorCount)
        {
            return processorCount == 0 ? 0 : (1 << processorCount) - 1;
        }

        private static ProcessPriorityClass Map(CpuPriorityClass value)
        {
            return value switch
            {
                CpuPriorityClass.Low => ProcessPriorityClass.Idle,
                CpuPriorityClass.BelowNormal => ProcessPriorityClass.BelowNormal,
                CpuPriorityClass.Normal => ProcessPriorityClass.Normal,
                CpuPriorityClass.AboveNormal => ProcessPriorityClass.AboveNormal,
                CpuPriorityClass.High => ProcessPriorityClass.High,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
    }
}
