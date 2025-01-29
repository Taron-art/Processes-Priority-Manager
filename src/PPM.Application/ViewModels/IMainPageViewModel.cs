using System.ComponentModel;
using Affinity_manager.Model;
using Affinity_manager.ViewWrappers;
using CommunityToolkit.Mvvm.Input;

namespace Affinity_manager.ViewModels
{
    public interface IMainPageViewModel : IShowsMessages, INotifyPropertyChanged
    {
        string? NewProcessName { get; set; }
        IReadOnlyObservableCollection<ProcessConfigurationView> ProcessesConfigurations { get; }
        ProcessConfigurationView? SelectedView { get; set; }
        bool IsSaveAvailable { get; }
        bool IsInterfaceVisible { get; }
        bool ApplyOnRunningProcesses { get; set; }

        public IRelayCommand AddCommand { get; }

        public IAsyncRelayCommand SaveChangesCommand { get; }

        public IAsyncRelayCommand ReloadCommand { get; }
        bool IsCancelAvailable { get; }

        ProcessInfoView[] GetAutoCompleteList();
    }
}
