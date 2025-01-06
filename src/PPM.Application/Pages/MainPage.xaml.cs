using System;
using Affinity_manager.ViewModels;
using Affinity_manager.ViewWrappers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Affinity_manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public IMainPageViewModel? ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is IMainPageViewModel model)
            {
                if (ViewModel != null)
                {
                    ViewModel.ShowMessage -= ViewModelShowMessage;
                }

                ViewModel = model;
                ViewModel.ShowMessage += ViewModelShowMessage;
            }

            base.OnNavigatedTo(e);
        }

        private void KeyboardAccelerator_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            ViewModel!.AddCommand.Execute(null);
        }

        private void processesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (processesGrid.SelectedItem != null)
            {
                processesGrid.ScrollIntoView(processesGrid.SelectedItem);
            }
        }

        private async void affinityButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            AffinitySelectorDialog dialog = new AffinitySelectorDialog((AffinityView)((Button)sender).DataContext);
            dialog.XamlRoot = XamlRoot;
            await dialog.ShowAsync();
        }

        private async void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (!ViewModel!.IsInterfaceVisible)
            {
                ContentDialog messageDialog = new()
                {
                    Content = "Probable running from restricted area detected. Application will exit",
                    PrimaryButtonText = "ОК",
                    XamlRoot = XamlRoot,
                };

                await messageDialog.ShowAsync();
                Environment.Exit(0);
            }
        }

        private async void Page_Loading(Microsoft.UI.Xaml.FrameworkElement sender, object args)
        {
            if (ViewModel == null)
            {
                throw new InvalidOperationException("ViewModel is not set");
            }

            if (ViewModel.ReloadCommand.CanExecute(null))
            {
                await ViewModel.ReloadCommand.ExecuteAsync(null);
            }
        }

        private async void ViewModelShowMessage(object? sender, string message)
        {
            ContentDialog messageDialog = new()
            {
                Title = Strings.PPM.Error,
                Content = message,
                CloseButtonText = Strings.PPM.OK,
                XamlRoot = XamlRoot
            };
            await messageDialog.ShowAsync();
        }
    }
}
