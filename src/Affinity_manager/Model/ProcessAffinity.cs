using CommunityToolkit.Mvvm.ComponentModel;

namespace Affinity_manager.Model
{
    public sealed partial class ProcessAffinity : ObservableObject
    {
        public const uint AffinityDefaultValue = uint.MaxValue;
        public const CpuPriorityClass CpuPriorityDefaultValue = CpuPriorityClass.Normal;
        public const IoPriority IoPriorityDefaultValue = IoPriority.Normal;


        [ObservableProperty]
        private uint _cpuAffinityMask = AffinityDefaultValue;

        [ObservableProperty]
        public CpuPriorityClass _cpuPriority = CpuPriorityDefaultValue;

        [ObservableProperty]
        public IoPriority _ioPriority = IoPriorityDefaultValue;

        public ProcessAffinity(string name)
        {
            Name = name;
            IoPriority = IoPriority.Normal;
        }

        public string Name { get; }


        public bool IsEmpty
        {
            get
            {
                return CpuAffinityMask == AffinityDefaultValue && IoPriority == IoPriorityDefaultValue && CpuPriority == CpuPriorityDefaultValue;
            }
        }

        public void Reset()
        {
            CpuAffinityMask = AffinityDefaultValue;
            IoPriority = IoPriorityDefaultValue;
            CpuPriority = CpuPriorityDefaultValue;
        }
    }
}
