using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Affinity_manager.Exceptions;
using Affinity_manager.Model;
using Affinity_manager.Model.CRUD;
using Affinity_manager.Utils;
using Affinity_manager.ViewWrappers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Affinity_manager.ViewModels
{
    public partial class MainPageViewModel : ObservableObject, IShowsMessages
    {
        private readonly ProcessConfigurationEqualityComparer _affinityEqualityComparer = new();

        [ObservableProperty]
        private string? _newProcessName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SaveCancelAvailable))]
        private BindingCollectionWithUniqunessCheck<ProcessConfigurationView> _processAffinities = new BindingCollectionWithUniqunessCheck<ProcessConfigurationView>();

        [ObservableProperty]
        private CpuPriorityClass _cpuPriority = CpuPriorityClass.Normal;

        [ObservableProperty]
        private ProcessConfigurationView? _selectedView;

        public MainPageViewModel()
        {
            OptionsProvider = new OptionsProvider();
            Repository = new ProcessConfigurationsRepository();
        }

        public bool SaveCancelAvailable => ProcessAffinities.Any(item => item.IsDirty);

        public bool IsInterfaceVisible
        {
            get
            {
                return RusDetector.Check();
            }
        }

        internal OptionsProvider OptionsProvider { get; }

        private ProcessConfigurationsRepository Repository { get; }

        [RelayCommand]
        public void Add()
        {
            string? processName = NewProcessName?.Trim();
            if (string.IsNullOrEmpty(processName))
            {
                return;
            }

            ProcessConfiguration processAffinity = new(processName);
            ProcessConfigurationView viewItem = new(processAffinity, OptionsProvider);
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
        public async Task SaveChangesAsync()
        {
            try
            {
                await Repository.SaveAsync(ProcessAffinities.Where(item => item.IsDirty).Select(item => item.ProcessConfiguration), FillProcesses);
            }
            catch (ServiceNotInstalledException)
            {
                OnShowMessage(Strings.Resources.ServiceNotFountErrorMessage);
            }
        }

        [RelayCommand]
        public Task ReloadAsync()
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

            ProcessAffinities = await Task.Run(() => new BindingCollectionWithUniqunessCheck<ProcessConfigurationView>(
                Repository.Get().Select(process => new ProcessConfigurationView(process, OptionsProvider)),
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

        partial void OnProcessAffinitiesChanged(BindingCollectionWithUniqunessCheck<ProcessConfigurationView>? oldValue, BindingCollectionWithUniqunessCheck<ProcessConfigurationView> newValue)
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

        private void OnShowMessage(string message)
        {
            ShowMessage?.Invoke(this, message);
        }

        public event EventHandler<string>? ShowMessage;
    }
}
