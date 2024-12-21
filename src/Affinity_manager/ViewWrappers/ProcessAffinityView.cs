using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Affinity_manager.Model;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Affinity_manager.ViewWrappers
{
    public partial class ProcessAffinityView : ObservableObject, IComparable<ProcessAffinityView>
    {
        private bool _isDirty = false;

        public ProcessAffinityView(ProcessAffinity processAffinity, OptionsProvider optionsProvider)
        {
            ProcessAffinity = processAffinity;
            OptionsProvider = optionsProvider;
            AffinityView = new AffinityView(processAffinity.CpuAffinityMask, 16);

            ProcessAffinity.PropertyChanged += ProcessAffinity_PropertyChanged;
            AffinityView.PropertyChanged += AffinityView_PropertyChanged;
        }

        public ProcessAffinity ProcessAffinity { get; }
        public OptionsProvider OptionsProvider { get; }

        public string Name => ProcessAffinity.Name;

        public uint CpuAffinityMask => ProcessAffinity.CpuAffinityMask;

        public AffinityView AffinityView { get; }

        public IReadOnlyList<EnumViewWrapper<CpuPriorityClass>> CpuPriorities => OptionsProvider.CpuPriorities;

        public IReadOnlyList<EnumViewWrapper<IoPriority>> IoPriorities => OptionsProvider.IoPriorities;

        public EnumViewWrapper<IoPriority> IoPriority
        {
            get
            {
                return OptionsProvider.IoPriorities.First(wrapper => wrapper.Value.Equals(ProcessAffinity.IoPriority));
            }
            set
            {
                ProcessAffinity.IoPriority = value.Value;
            }
        }

        public EnumViewWrapper<CpuPriorityClass> CpuPriority
        {
            get
            {
                return OptionsProvider.CpuPriorities.First(wrapper => wrapper.Value == ProcessAffinity.CpuPriority);
            }
            set
            {
                ProcessAffinity.CpuPriority = value.Value;
            }
        }

        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            private set
            {
                SetProperty(ref _isDirty, value);
            }
        }

        public int CompareTo(ProcessAffinityView? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;

            return Name.CompareTo(other.Name);
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }

        private void ProcessAffinity_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            IsDirty = true;

            switch (e.PropertyName)
            {
                case nameof(ProcessAffinity.CpuPriority):
                    OnPropertyChanged(nameof(CpuPriority));
                    break;
                case nameof(ProcessAffinity.IoPriority):
                    OnPropertyChanged(nameof(IoPriority));
                    break;
                case nameof(ProcessAffinity.CpuAffinityMask):
                    OnPropertyChanged(nameof(CpuAffinityMask));
                    break;
            }
        }


        private void AffinityView_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AffinityView.AffinityMask):
                    ProcessAffinity.CpuAffinityMask = AffinityView.AffinityMask;
                    break;
            }
        }
    }
}
