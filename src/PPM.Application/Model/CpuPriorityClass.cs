using System.ComponentModel.DataAnnotations;

namespace Affinity_manager.Model
{
    public enum CpuPriorityClass : uint
    {
        [Display(Order = 0)]
        Low = 1,
        [Display(Order = 1)]
        BelowNormal = 5,
        [Display(Order = 2)]
        Normal = 2,
        [Display(Order = 3)]
        AboveNormal = 6,
        [Display(Order = 4)]
        High = 3,
    }
}
