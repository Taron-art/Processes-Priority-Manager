using System;
using System.ComponentModel.DataAnnotations;
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
    public partial class MainPageViewModel(IProcessConfigurationsRepository repository, IProcessConfigurationViewFactory viewFactory, IAutocompleteProvider autocompleteView)
        : ObservableObject, IMainPageViewModel
    {
        [ObservableProperty]
        private string? _newProcessName;

        private BindingCollectionWithUniqunessCheck<ProcessConfigurationView> _processesConfigurations = viewFactory.CreateCollection([]);

        [ObservableProperty]
        private ProcessConfigurationView? _selectedView;

        private IProcessConfigurationsRepository Repository { get; } = repository;

        public IProcessConfigurationViewFactory ViewFactory { get; } = viewFactory;
        public IAutocompleteProvider AutocompleteProvider { get; } = autocompleteView;

        public IReadOnlyObservableCollection<ProcessConfigurationView> ProcessesConfigurations
        {
            get
            {
                return _processesConfigurations;
            }
        }

        public bool SaveCancelAvailable => ProcessesConfigurations.Any(item => item.IsDirty);

        public bool IsInterfaceVisible
        {
            get
            {
                return RusDetector.Check();
            }
        }

        public ProcessInfoView[] GetAutoCompleteList()
        {
            return AutocompleteProvider.GetAutocompleteList(NewProcessName);
        }

        [RelayCommand]
        public void Add()
        {
            string? processName = NewProcessName?.Trim();
            if (string.IsNullOrEmpty(processName))
            {
                return;
            }

            ProcessConfiguration processAffinity = new(processName);

            if (processAffinity.HasErrors)
            {
                // WinUI does not support validation out of box... To not invent a wheel for one text box, we just show a message.
                OnShowMessage(processAffinity.GetErrors().First().ErrorMessage ?? Strings.PPM.UnknownErrorMessage);
                return;
            }

            ProcessConfigurationView viewItem = ViewFactory.Create(processAffinity);
            viewItem.MarkDirty();
            if (!_processesConfigurations.TryAddItem(viewItem))
            {
                SelectedView = ProcessesConfigurations.Single(item => item.Name == viewItem.Name);
            }
            else
            {
                SelectedView = viewItem;
            }
            NewProcessName = string.Empty;
            AutocompleteProvider.AddProcesses([processName]);
            AutocompleteProvider.ClearCache();
        }

        [RelayCommand]
        public async Task SaveChangesAsync()
        {
            bool fillProcessesCalled = false;
            try
            {

                await Repository.SaveAndRestartServiceAsync(ProcessesConfigurations.Where(item => item.IsDirty).Select(
                    item => item.ProcessConfiguration),
                    () =>
                    {
                        fillProcessesCalled = true;
                        return FillProcesses();
                    });

                if (!fillProcessesCalled)
                {
                    await FillProcesses();
                }
            }
            catch (ValidationException e)
            {
                OnShowMessage(e.Message);
            }
            catch (ServiceNotInstalledException)
            {
                OnShowMessage(Strings.PPM.ServiceNotFountErrorMessage);
            }
        }

        [RelayCommand]
        public Task ReloadAsync()
        {
            return FillProcesses();
        }

        private async Task FillProcesses()
        {
            AutocompleteProvider.ClearCache();
            string? selectedProcessName = null;
            if (SelectedView != null)
            {
                selectedProcessName = SelectedView.Name;
            }

            SetProcessAffinities(await Task.Run(() => ViewFactory.CreateCollection(Repository.Get())));

            if (selectedProcessName != null)
            {
                SelectedView = ProcessesConfigurations.SingleOrDefault(item => item.Name == selectedProcessName);
            }
            else
            {
                SelectedView = ProcessesConfigurations.FirstOrDefault();
            }
        }

        private void SetProcessAffinities(BindingCollectionWithUniqunessCheck<ProcessConfigurationView> value)
        {
            if (_processesConfigurations != null)
            {
                _processesConfigurations.CollectionChanged -= OnProcessAffinitiesItemsChanged;
                _processesConfigurations.ItemChanged -= OnProcessAffinitiesItemsChanged;
            }
            _processesConfigurations = value;

            if (_processesConfigurations != null)
            {
                _processesConfigurations.CollectionChanged += OnProcessAffinitiesItemsChanged;
                _processesConfigurations.ItemChanged += OnProcessAffinitiesItemsChanged;
                AutocompleteProvider.AddProcesses(_processesConfigurations.Select(item => item.Name));
            }

            OnPropertyChanged(nameof(ProcessesConfigurations));
            OnPropertyChanged(nameof(SaveCancelAvailable));
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
