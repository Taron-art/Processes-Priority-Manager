using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Affinity_manager.Model;
using Affinity_manager.Utils;
using Affinity_manager.ViewWrappers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Affinity_manager
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly ProcessAffinityEqualityComparer _affinityEqualityComparer = new();

        [ObservableProperty]
        private string? _newProcessName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SaveCancelAvailable))]
        private BindingCollectionWithUniqunessCheck<ProcessAffinityView> _processAffinities = new BindingCollectionWithUniqunessCheck<ProcessAffinityView>();

        [ObservableProperty]
        private CpuPriorityClass _cpuPriority = CpuPriorityClass.Normal;

        [ObservableProperty]
        private ProcessAffinityView? _selectedView;

        public MainPageViewModel()
        {
            OptionsProvider = new OptionsProvider();
            ProcessAffinitiesManager = new ProcessAffinitiesManager();
        }

        public bool SaveCancelAvailable => ProcessAffinities.Any(item => item.IsDirty);

        internal OptionsProvider OptionsProvider { get; }
        internal ProcessAffinitiesManager ProcessAffinitiesManager { get; }

        public bool IsInterfaceVisible
        {
            get
            {
                return RusDetector.Check();
            }
        }

        [RelayCommand]
        public void Add()
        {
            string? processName = NewProcessName?.Trim();
            if (string.IsNullOrEmpty(processName))
            {
                return;
            }

            ProcessAffinity processAffinity = new(processName);
            ProcessAffinityView viewItem = new(processAffinity, OptionsProvider);
            viewItem.MarkDirty();
            if (!ProcessAffinities.TryAddItem(viewItem))
            {
                SelectedView = ProcessAffinities.Single(item => item.Name == viewItem.Name);
            }
            else
            {
                SelectedView = viewItem;
            }
            NewProcessName = string.Empty;
        }

        [RelayCommand]
        public async Task SaveChanges()
        {
            await Task.Run(() => ProcessAffinitiesManager.SaveToRegistry(ProcessAffinities.Where(item => item.IsDirty).Select(item => item.ProcessAffinity)));
            await FillProcesses();
        }

        [RelayCommand]
        public Task Reload()
        {
            return FillProcesses();
        }

#pragma warning disable MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
        [MemberNotNull(nameof(_processAffinities))]
#pragma warning restore MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
        private async Task FillProcesses()
        {
            string? selectedProcessName = null;
            if (SelectedView != null)
            {
                selectedProcessName = SelectedView.Name;
            }

            ProcessAffinities = await Task.Run(() => new BindingCollectionWithUniqunessCheck<ProcessAffinityView>(
                ProcessAffinitiesManager.LoadFromRegistry().Select(process => new ProcessAffinityView(process, OptionsProvider)),
                _affinityEqualityComparer));

            if (selectedProcessName != null)
            {
                SelectedView = ProcessAffinities.SingleOrDefault(item => item.Name == selectedProcessName);
            }
            else
            {
                SelectedView = ProcessAffinities.FirstOrDefault();
            }
        }

        partial void OnProcessAffinitiesChanged(BindingCollectionWithUniqunessCheck<ProcessAffinityView>? oldValue, BindingCollectionWithUniqunessCheck<ProcessAffinityView> newValue)
        {
            if (oldValue != null)
            {
                oldValue.CollectionChanged -= OnProcessAffinitiesItemsChanged;
                oldValue.ItemChanged -= OnProcessAffinitiesItemsChanged;
            }

            newValue.CollectionChanged += OnProcessAffinitiesItemsChanged;
            newValue.ItemChanged += OnProcessAffinitiesItemsChanged;
        }

        private void OnProcessAffinitiesItemsChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(SaveCancelAvailable));
        }
    }
}
