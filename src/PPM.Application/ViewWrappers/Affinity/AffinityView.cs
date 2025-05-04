using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPM.Unsafe;

namespace Affinity_manager.ViewWrappers.Affinity
{
    public partial class AffinityView : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FriendlyView))]
        private ulong _affinityMask;
        private readonly uint _numberOfLogicalCpus;
        private string? _friendlyView;
        private bool _changingCoreValue;
        private IGrouping<PhysicalCoreGroup, (CoreView Core, PhysicalCoreGroup Group)>[]? _smtRelatedCores;
        private readonly CoreView[] _coreViews;
        private GroupsView<CacheCoreGroup>? _cacheGroupView;
        private GroupsView<PerformanceCoreGroup>? _performanceGroupViews;

        public AffinityView(ulong affinityMask, IReadOnlyList<CoreInfo> coresInfo)
        {
            ArgumentOutOfRangeException.ThrowIfZero(coresInfo.Count, nameof(coresInfo));

            _affinityMask = affinityMask;
            CoresInfo = coresInfo;
            _numberOfLogicalCpus = (uint)coresInfo.Count;

            CoreView[] logicalCpus = new CoreView[(int)_numberOfLogicalCpus];

            FillBoolArray(logicalCpus, _numberOfLogicalCpus);
            _coreViews = logicalCpus;
        }

        public IReadOnlyList<CoreInfo> CoresInfo { get; }

        public IReadOnlyList<CoreView> LogicalCpus
        {
            get => _coreViews;
        }

        public string FriendlyView
        {
            get
            {
                return _friendlyView ??= ConvertBoolsToString(LogicalCpus);
            }
        }

        public bool AllCpus
        {
            get
            {
                return LogicalCpus.All(coreView => coreView.DefinetelySelected);
            }
            set
            {
                if (_changingCoreValue)
                {
                    return;
                }

                foreach (CoreView coreView in LogicalCpus)
                {
                    coreView.Selected = value;
                }
                OnPropertyChanged(nameof(AllCpus));
                OnPropertyChanged(nameof(LogicalCpus));
                OnPropertyChanged(nameof(SelectSmtCores));
            }
        }

        public bool ShowSmtCores
        {
            get
            {
#if DEBUG
                return true;
#else
                return GetSmtRelatedCoreGroups().Any();
#endif
            }
        }

        public bool? SelectSmtCores
        {
            get
            {
                IGrouping<PhysicalCoreGroup, (CoreView Core, PhysicalCoreGroup Group)>[] smtRelatedGroups = GetSmtRelatedCoreGroups();

                uint positiveGroups = 0;
                uint negativeGroups = 0;
                foreach (IGrouping<PhysicalCoreGroup, (CoreView Core, PhysicalCoreGroup Group)>? group in smtRelatedGroups)
                {
                    // It was arbitrary decided that "real" core will be the last one in the group.
                    if (group.Reverse().Skip(1).All(item => item.Core.DefinetelySelected))
                    {
                        positiveGroups++;
                    }
                    else
                    {
                        negativeGroups++;
                    }
                }

                if (positiveGroups == smtRelatedGroups.Length && positiveGroups > 0)
                {
                    return true;
                }
                else if (negativeGroups == smtRelatedGroups.Length)
                {
                    return false;
                }

                return null;
            }
            set
            {
                if (_changingCoreValue)
                {
                    return;
                }

                if (value == null)
                {
                    return;
                }

                IGrouping<PhysicalCoreGroup, (CoreView Core, PhysicalCoreGroup Group)>[] smtRelatedGroups = GetSmtRelatedCoreGroups();
                foreach (IGrouping<PhysicalCoreGroup, (CoreView Core, PhysicalCoreGroup Group)> group in smtRelatedGroups)
                {
                    // It was arbitrary decided that "real" core will be the last one in the group.
                    foreach ((CoreView Core, PhysicalCoreGroup Group) item in group.Reverse().Skip(1))
                    {
                        item.Core.Selected = value.Value;
                    }
                }
                OnPropertyChanged(nameof(SelectSmtCores));
                OnPropertyChanged(nameof(LogicalCpus));
            }
        }

        private IGrouping<PhysicalCoreGroup, (CoreView Core, PhysicalCoreGroup Group)>[] GetSmtRelatedCoreGroups()
        {
            return _smtRelatedCores ??= LogicalCpus.Select(coreView => (Core: coreView, Group: coreView.CoreInfo.AssociatedGroups.OfType<PhysicalCoreGroup>().SingleOrDefault())).Where(group => group.Group != null).GroupBy(tuple => tuple.Group).Where(group => group.Count() > 1).ToArray()!;
        }

        public GroupsView<CacheCoreGroup> CacheGroupView
        {
            get
            {
                return _cacheGroupView ??= CreateCacheGroupView();
            }
        }

        public GroupsView<PerformanceCoreGroup> PerformanceGroupView
        {
            get
            {
                return _performanceGroupViews ??= CreatePerformanceGroupView();
            }
        }


        public bool CanAccept
        {
            get
            {
                return LogicalCpus.Any(coreView => coreView.DefinetelySelected);
            }
        }

        public void UpdateAffinityMask(ulong affinityMask)
        {
            AffinityMask = affinityMask;
        }

        [RelayCommand]
        public void ApplyChanges()
        {
            UpdateAffinityMask(LogicalCpus);
        }

        [RelayCommand]
        public void CancelChanges()
        {
            FillBoolArray(_coreViews, _numberOfLogicalCpus);
            OnPropertyChanged(nameof(LogicalCpus));
        }

        partial void OnAffinityMaskChanged(ulong value)
        {
            _friendlyView = null;
            FillBoolArray(_coreViews, _numberOfLogicalCpus);
        }

        private void UpdateAffinityMask(IReadOnlyList<CoreView> logicalCpus)
        {
            ulong affinityMask = 0;
            for (int i = 0; i < logicalCpus.Count; i++)
            {
                if (logicalCpus[i].DefinetelySelected)
                {
                    affinityMask |= 1u << i;
                }
            }

            for (int i = logicalCpus.Count; i < sizeof(ulong) * 8; i++)
            {
                affinityMask |= 1ul << i;
            }

            AffinityMask = affinityMask;
        }

        private void FillBoolArray(CoreView[] logicalCpus, in uint numberOfLogicalCpus)
        {
            for (int i = 0; i < numberOfLogicalCpus; i++)
            {
                bool value = (AffinityMask & (1u << i)) != 0;
                if (logicalCpus[i] == null)
                {
                    logicalCpus[i] = new CoreView(value, CoresInfo[i]);
                    logicalCpus[i].PropertyChanged += CoreView_PropertyChanged;
                }
                else
                {
                    logicalCpus[i].Selected = value;
                }
            }
        }

        private GroupsView<PerformanceCoreGroup> CreatePerformanceGroupView()
        {
            GroupsView<PerformanceCoreGroup> view = new(
                GetCoreGroups<PerformanceCoreGroup, GroupWithRegularLabel<PerformanceCoreGroup>>(
                                    _coreViews,
                                    group => new GroupWithRegularLabel<PerformanceCoreGroup>() { CoreGroup = group }));
            view.UpdateGroupsState(_coreViews);
            view.GroupChanged += OnPerformanceGroupChanged;
            return view;
        }

        private void OnPerformanceGroupChanged(object? sender, GroupChangedEventArgs<PerformanceCoreGroup> e)
        {
            if (e.CoreGroupView.Selected == null)
            {
                return;
            }

            foreach (CoreView coreView in LogicalCpus.Where(cpu => cpu.CoreInfo.AssociatedGroups.Contains(e.CoreGroupView.CoreGroup)))
            {
                coreView.Selected = e.CoreGroupView.Selected;
            }
        }

        private GroupsView<CacheCoreGroup> CreateCacheGroupView()
        {
            GroupsView<CacheCoreGroup> view = new(
                GetCoreGroups<CacheCoreGroup, GroupWithCacheLabel>(
                                    _coreViews,
                                    group => new GroupWithCacheLabel() { CoreGroup = group },
                                    group => group.Level == 3));
            view.UpdateGroupsState(_coreViews);
            view.GroupChanged += OnCacheGroupChanged;
            return view;
        }

        private void OnCacheGroupChanged(object? sender, GroupChangedEventArgs<CacheCoreGroup> e)
        {
            if (e.CoreGroupView.Selected == null)
            {
                return;
            }

            foreach (CoreView coreView in LogicalCpus.Where(cpu => cpu.CoreInfo.AssociatedGroups.Contains(e.CoreGroupView.CoreGroup)))
            {
                coreView.Selected = e.CoreGroupView.Selected;
            }
        }

        private static TGroupView[] GetCoreGroups<TGroup, TGroupView>(CoreView[] coreViews, Func<TGroup, TGroupView> creator, Func<TGroup, bool>? filter = null)
            where TGroup : CoreGroup
            where TGroupView : CoreGroupView<TGroup>
        {
            return coreViews
                .SelectMany(coreView => coreView.CoreInfo.AssociatedGroups)
                .OfType<TGroup>()
                .Where(group => filter == null || filter(group))
                .Distinct()
                .Order()
                .Select(cache => creator(cache)).ToArray();
        }

        private static string ConvertBoolsToString(IReadOnlyList<CoreView> bools)
        {
            List<int> indexes = new(bools.Count);

            bool allCpus = true;

            for (int i = 0; i < bools.Count; i++)
            {
                if (bools[i].DefinetelySelected)
                {
                    indexes.Add(i);
                }

                allCpus &= bools[i].DefinetelySelected;
            }

            if (allCpus)
            {
                return Strings.PPM.AllCpus;
            }

            List<string> result = [];
            int start = -1, end = -1;

            for (int i = 0; i < indexes.Count; i++)
            {
                if (start == -1)
                {
                    start = indexes[i];
                    end = start;
                }
                else if (indexes[i] == end + 1)
                {
                    end = indexes[i];
                }
                else
                {
                    if (end - start > 1)
                    {
                        result.Add(string.Format(Strings.PPM.CpuRangeFormat, start, end));
                    }
                    else
                    {
                        result.Add(string.Format(Strings.PPM.CpuFormat, start));
                        if (start != end)
                        {
                            result.Add(string.Format(Strings.PPM.CpuFormat, end));
                        }
                    }
                    start = indexes[i];
                    end = start;
                }
            }

            if (start != -1)
            {
                if (end - start > 1)
                {
                    result.Add(string.Format(Strings.PPM.CpuRangeFormat, start, end));
                }
                else
                {
                    result.Add(string.Format(Strings.PPM.CpuFormat, start));
                    if (start != end)
                    {
                        result.Add(string.Format(Strings.PPM.CpuFormat, end));
                    }
                }
            }

            return string.Join(", ", result);
        }


        private void CoreView_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CoreView.Selected):
                    _changingCoreValue = true;
                    try
                    {
                        OnPropertyChanged(nameof(AllCpus));
                        OnPropertyChanged(nameof(CanAccept));
                        OnPropertyChanged(nameof(SelectSmtCores));
                        PerformanceGroupView.UpdateGroupsState(_coreViews);
                        CacheGroupView.UpdateGroupsState(_coreViews);
                    }
                    finally
                    {
                        _changingCoreValue = false;
                    }
                    break;

            }
        }
    }
}
