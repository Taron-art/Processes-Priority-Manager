using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Affinity_manager.Model
{
    public sealed partial class ProcessConfiguration : ObservableValidator
    {
        public const ulong AffinityDefaultValue = ulong.MaxValue;
        public const CpuPriorityClass CpuPriorityDefaultValue = CpuPriorityClass.Normal;
        public const IoPriority IoPriorityDefaultValue = IoPriority.Normal;
        public const PagePriority MemoryPriorityDefaultValue = PagePriority.Normal;

        [ObservableProperty]
        private ulong _cpuAffinityMask = AffinityDefaultValue;

        [ObservableProperty]
        public CpuPriorityClass _cpuPriority = CpuPriorityDefaultValue;

        [ObservableProperty]
        public IoPriority _ioPriority = IoPriorityDefaultValue;

        [ObservableProperty]
        public PagePriority _memoryPriority = MemoryPriorityDefaultValue;

        public ProcessConfiguration(string name)
        {
            Name = name;
            ValidateAllProperties();
        }

        [FileExtensions(Extensions = ".exe", ErrorMessageResourceType = typeof(Strings.Validation), ErrorMessageResourceName = nameof(Strings.Validation.ProcessNameMustHaveExeExtension))]
        public string Name { get; }


        public bool IsEmpty
        {
            get
            {
                return CpuAffinityMask == AffinityDefaultValue
                    && IoPriority == IoPriorityDefaultValue
                    && CpuPriority == CpuPriorityDefaultValue
                    && MemoryPriority == MemoryPriorityDefaultValue;
            }
        }

        public void Reset()
        {
            CpuAffinityMask = AffinityDefaultValue;
            IoPriority = IoPriorityDefaultValue;
            CpuPriority = CpuPriorityDefaultValue;
            MemoryPriority = MemoryPriorityDefaultValue;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
