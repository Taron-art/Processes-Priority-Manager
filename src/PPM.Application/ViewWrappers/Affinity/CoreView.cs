using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Affinity_manager.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using PPM.Unsafe;

namespace Affinity_manager.ViewWrappers.Affinity
{
    [DebuggerDisplay($"{nameof(Label)} - {nameof(DefinetelySelected)}")]
    public partial class CoreView : ObservableObject, IComparable<CoreView>, ICoreView
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DefinetelySelected))]
        private bool? _selected;

        public CoreView(bool value, CoreInfo coreInfo)
        {
            _selected = value;
            CoreInfo = coreInfo;
            Label = string.Format(Strings.PPM.CpuFormat, coreInfo.Id);
        }

        public string Label { get; }

        public CoreInfo CoreInfo { get; }

        public bool DefinetelySelected => Selected ?? false;

        public string? Description => new Lazy<string>(CreateDescription).Value;

        private string CreateDescription()
        {
            StringBuilder stringBuilder = new();

            PhysicalCoreGroup? coreGroup = CoreInfo.AssociatedGroups.OfType<PhysicalCoreGroup>().SingleOrDefault();
            if (coreGroup != null)
            {
                stringBuilder.AppendLine(string.Format(Strings.PPM.PhysicalCoreGroupFormat, coreGroup.Id));
            }

            PerformanceCoreGroup? performanceClass = CoreInfo.AssociatedGroups.OfType<PerformanceCoreGroup>().SingleOrDefault();
            if (performanceClass != null)
            {
                stringBuilder.AppendLine(string.Format(Strings.PPM.PerformanceClassFormat, performanceClass.Id));
            }

            for (int i = 1; i <= 3; i++)
            {
                CacheCoreGroup[] cacheGroup = CoreInfo.AssociatedGroups.OfType<CacheCoreGroup>().Where(group => group.Level == i).ToArray();
                if (cacheGroup.Length == 0)
                {
                    continue;
                }
                stringBuilder.Append(string.Format(Strings.PPM.CacheLevelFormat, i)).Append(" ");

                for (int j = 0; j < cacheGroup.Length; j++)
                {
                    if (j == 0)
                    {
                        stringBuilder.Append(SizeFormatHelper.SizeToString(cacheGroup[j].CacheSizeInB));
                    }
                    else
                    {
                        stringBuilder.Append(string.Format(Strings.PPM.CacheAdditionFormat, SizeFormatHelper.SizeToString(cacheGroup[j].CacheSizeInB)));
                    }

                    if (j == cacheGroup.Length - 1)
                    {
                        stringBuilder.AppendLine();
                    }
                }
            }

            return stringBuilder.ToString();
        }

        public int CompareTo(CoreView? other)
        {
            if (other == null) return 1;
            return Label.CompareTo(other.Label);
        }
    }
}
