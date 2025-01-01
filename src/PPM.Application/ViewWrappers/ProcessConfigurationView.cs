using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Affinity_manager.Model;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Affinity_manager.ViewWrappers
{
    public partial class ProcessConfigurationView : ObservableObject, IComparable<ProcessConfigurationView>
    {
        private bool _isDirty = false;

        public ProcessConfigurationView(ProcessConfiguration configuration, OptionsProvider optionsProvider)
        {
            ProcessConfiguration = configuration;
            OptionsProvider = optionsProvider;
            AffinityView = new AffinityView(configuration.CpuAffinityMask, optionsProvider.NumberOfLogicalCpus);

            ProcessConfiguration.PropertyChanged += OnProcessConfigurationPropertyChanged;
            AffinityView.PropertyChanged += OnAffinityViewPropertyChanged;
        }

        public ProcessConfiguration ProcessConfiguration { get; }
        public OptionsProvider OptionsProvider { get; }

        public string Name => ProcessConfiguration.Name;

        public ulong CpuAffinityMask => ProcessConfiguration.CpuAffinityMask;

        public AffinityView AffinityView { get; }

        public IReadOnlyList<EnumViewWrapper<CpuPriorityClass>> CpuPriorities => OptionsProvider.CpuPriorities;

        public IReadOnlyList<EnumViewWrapper<IoPriority>> IoPriorities => OptionsProvider.IoPriorities;

        public EnumViewWrapper<IoPriority> IoPriority
        {
            get
            {
                return OptionsProvider.IoPriorities.First(wrapper => wrapper.Value.Equals((object)ProcessConfiguration.IoPriority));
            }
            set
            {
                ProcessConfiguration.IoPriority = value.Value;
            }
        }

        public EnumViewWrapper<CpuPriorityClass> CpuPriority
        {
            get
            {
                return OptionsProvider.CpuPriorities.First(wrapper => wrapper.Value == ProcessConfiguration.CpuPriority);
            }
            set
            {
                ProcessConfiguration.CpuPriority = value.Value;
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

        public int CompareTo(ProcessConfigurationView? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;

            return Name.CompareTo(other.Name);
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }

        private void OnProcessConfigurationPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            IsDirty = true;

            switch (e.PropertyName)
            {
                case nameof(ProcessConfiguration.CpuPriority):
                    OnPropertyChanged(nameof(CpuPriority));
                    break;
                case nameof(ProcessConfiguration.IoPriority):
                    OnPropertyChanged(nameof(IoPriority));
                    break;
                case nameof(ProcessConfiguration.CpuAffinityMask):
                    OnPropertyChanged(nameof(CpuAffinityMask));
                    break;
            }
        }


        private void OnAffinityViewPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AffinityView.AffinityMask):
                    ProcessConfiguration.CpuAffinityMask = AffinityView.AffinityMask;
                    break;
            }
        }
    }
}
