using System.Collections.Generic;
using System.Linq;
using Affinity_manager.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Affinity_manager.ViewWrappers
{
    public partial class AffinityView : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FriendlyView))]
        private uint _affinityMask;
        private readonly uint _numberOfLogicalCpus;
        private string? _friendlyView;
        private bool _changingCoreValue;
        private readonly CoreView[] _coreViews;

        public AffinityView(uint affinityMask, uint numberOfLogicalCpus)
        {
            _affinityMask = affinityMask;
            _numberOfLogicalCpus = numberOfLogicalCpus;

            var logicalCpus = new CoreView[(int)numberOfLogicalCpus];

            FillBoolArray(logicalCpus, numberOfLogicalCpus);
            _coreViews = logicalCpus;
        }

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
                return LogicalCpus.All(coreView => coreView.Value);
            }
            set
            {
                if (_changingCoreValue)
                {
                    return;
                }

                foreach (CoreView coreView in LogicalCpus)
                {
                    coreView.Value = value;
                }
                OnPropertyChanged(nameof(AllCpus));
                OnPropertyChanged(nameof(LogicalCpus));
            }
        }

        public bool CanAccept
        {
            get
            {
                return LogicalCpus.Any(coreView => coreView.Value);
            }
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

        partial void OnAffinityMaskChanged(uint value)
        {
            _friendlyView = null;
        }

        private void UpdateAffinityMask(IReadOnlyList<CoreView> logicalCpus)
        {
            uint affinityMask = 0;
            for (int i = 0; i < logicalCpus.Count; i++)
            {
                if (logicalCpus[i].Value)
                {
                    affinityMask |= 1u << i;
                }
            }

            for (int i = logicalCpus.Count; i < sizeof(uint) * 8; i++)
            {
                affinityMask |= 1u << i;
            }

            AffinityMask = affinityMask;
        }

        private void FillBoolArray(CoreView[] logicalCpus, in uint numberOfLogicalCpus)
        {
            for (int i = 0; i < numberOfLogicalCpus; i++)
            {
                var value = (AffinityMask & (1u << i)) != 0;
                if (logicalCpus[i] == null)
                {
                    logicalCpus[i] = new CoreView(value, string.Format(Resources.CpuFormat, i));
                    logicalCpus[i].PropertyChanged += CoreView_PropertyChanged;
                }
                else
                {
                    logicalCpus[i].Value = value;
                }
            }
        }

        private static string ConvertBoolsToString(IReadOnlyList<CoreView> bools)
        {
            List<int> indexes = new(bools.Count);

            bool allCpus = true;

            for (int i = 0; i < bools.Count; i++)
            {
                if (bools[i].Value)
                {
                    indexes.Add(i);
                }

                allCpus &= bools[i].Value;
            }

            if (allCpus)
            {
                return Resources.AllCpus;
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
                        result.Add(string.Format(Resources.CpuRangeFormat, start, end));
                    }
                    else
                    {
                        result.Add(string.Format(Resources.CpuFormat, start));
                        if (start != end)
                        {
                            result.Add(string.Format(Resources.CpuFormat, end));
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
                    result.Add(string.Format(Resources.CpuRangeFormat, start, end));
                }
                else
                {
                    result.Add(string.Format(Resources.CpuFormat, start));
                    if (start != end)
                    {
                        result.Add(string.Format(Resources.CpuFormat, end));
                    }
                }
            }

            return string.Join(", ", result);
        }


        private void CoreView_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CoreView.Value):
                    _changingCoreValue = true;
                    try
                    {
                        OnPropertyChanged(nameof(AllCpus));
                        OnPropertyChanged(nameof(CanAccept));
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
