using System.ComponentModel;
using System.Threading.Tasks;
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
        bool SaveCancelAvailable { get; }
        bool IsInterfaceVisible { get; }

        public IRelayCommand AddCommand { get; }

        public IAsyncRelayCommand SaveChangesCommand { get; }

        public IAsyncRelayCommand ReloadCommand { get; }
    }
}
