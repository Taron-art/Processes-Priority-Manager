using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Affinity_manager.Model;
using Affinity_manager.ViewWrappers.Affinity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Affinity_manager.ViewWrappers
{
    public partial class ProcessConfigurationView : ObservableObject, IComparable<ProcessConfigurationView>
    {
        private bool _isDirty = false;

        public ProcessConfigurationView(ProcessConfiguration configuration, IOptionsProvider optionsProvider, IProcessConfigurationApplier configurationApplier)
        {
            ProcessConfiguration = configuration;
            OptionsProvider = optionsProvider;
            ConfigurationApplier = configurationApplier;
            AffinityView = new AffinityView(configuration.CpuAffinityMask, optionsProvider.ProcessorCoresInfo);

            ProcessConfiguration.PropertyChanged += OnProcessConfigurationPropertyChanged;
            AffinityView.PropertyChanged += OnAffinityViewPropertyChanged;
        }

        public ProcessConfiguration ProcessConfiguration { get; }

        public IOptionsProvider OptionsProvider { get; }

        public IProcessConfigurationApplier ConfigurationApplier { get; }

        public string Name => ProcessConfiguration.Name;

        public ulong CpuAffinityMask => ProcessConfiguration.CpuAffinityMask;

        public AffinityView AffinityView { get; }

        public IReadOnlyList<EnumViewWrapper<CpuPriorityClass>> CpuPriorities => OptionsProvider.CpuPriorities;

        public IReadOnlyList<EnumViewWrapper<IoPriority>> IoPriorities => OptionsProvider.IoPriorities;

        public IReadOnlyList<EnumViewWrapper<PagePriority>> MemoryPriorities => OptionsProvider.MemoryPriorities;

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

        public EnumViewWrapper<PagePriority> MemoryPriority
        {
            get
            {
                return OptionsProvider.MemoryPriorities.First(wrapper => wrapper.Value == ProcessConfiguration.MemoryPriority);
            }
            set
            {
                ProcessConfiguration.MemoryPriority = value.Value;
            }
        }

        public bool? MemoryAndIoPrioritiesAreLowest
        {
            get
            {
                if (ProcessConfiguration.MemoryPriority == PagePriority.VeryLow && ProcessConfiguration.IoPriority == Model.IoPriority.VeryLow)
                {
                    return true;
                }
                else if (ProcessConfiguration.MemoryPriority == PagePriority.VeryLow || ProcessConfiguration.IoPriority == Model.IoPriority.VeryLow)
                {
                    return null; // This is treated by UI as "in the middle
                }

                return false;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                if (value.Value)
                {
                    ProcessConfiguration.MemoryPriority = PagePriority.VeryLow;
                    ProcessConfiguration.IoPriority = Model.IoPriority.VeryLow;
                }
                else
                {
                    ProcessConfiguration.MemoryPriority = ProcessConfiguration.MemoryPriorityDefaultValue;
                    ProcessConfiguration.IoPriority = ProcessConfiguration.IoPriorityDefaultValue;
                }
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

        public string? ToolTip
        {
            get
            {
                if (IsDirty)
                {
                    StringBuilder stringBuilder = new(Strings.PPM.ProcessorConfigurationModifiedToolTipStart);
                    stringBuilder.Append(' ');
                    if (ProcessConfiguration.IsEmpty)
                    {
                        stringBuilder.Append(Strings.PPM.ProcessorConfigurationModifiedToolTipEndDefaultState);
                    }
                    else
                    {
                        stringBuilder.Append(Strings.PPM.ProcessorConfigurationModifiedToolTipEnd);
                    }

                    return stringBuilder.ToString();
                }

                // We return null to hide the tooltip, with Empty it will be an empty tooltip.
                return null;
            }
        }

        public int CompareTo(ProcessConfigurationView? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;

            return Name.CompareTo(other.Name);
        }

        [RelayCommand]
        public void Reset()
        {
            ProcessConfiguration.Reset();
        }

        [RelayCommand]
        public async Task ApplyAsync()
        {
            byte numberOfLogicalCpus = OptionsProvider.NumberOfLogicalCpus <= 64 ? (byte)OptionsProvider.NumberOfLogicalCpus : (byte)0;
            await Task.Run(() => ConfigurationApplier.ApplyIfPresent(numberOfLogicalCpus, ProcessConfiguration));
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
                    OnPropertyChanged(nameof(MemoryAndIoPrioritiesAreLowest));
                    break;
                case nameof(ProcessConfiguration.CpuAffinityMask):
                    AffinityView.UpdateAffinityMask(CpuAffinityMask);
                    OnPropertyChanged(nameof(CpuAffinityMask));
                    break;
                case nameof(ProcessConfiguration.MemoryPriority):
                    OnPropertyChanged(nameof(MemoryPriority));
                    OnPropertyChanged(nameof(MemoryAndIoPrioritiesAreLowest));
                    break;
            }

            OnPropertyChanged(nameof(ToolTip));
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
