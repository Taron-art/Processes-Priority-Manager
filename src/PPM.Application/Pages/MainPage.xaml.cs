using System;
using Affinity_manager.ViewModels;
using Affinity_manager.ViewWrappers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Affinity_manager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public const int ColumnSpacing = 16;

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

        private async void AffinityButton_Click(object sender, RoutedEventArgs e)
        {
            AffinitySelectorDialog dialog = new AffinitySelectorDialog((AffinityView)((Button)sender).DataContext);
            dialog.XamlRoot = XamlRoot;
            await dialog.ShowAsync();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ViewModel!.IsInterfaceVisible)
            {
                // Intentionally not localized.
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

        private async void Page_Loading(FrameworkElement sender, object args)
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

        /// <summary>
        /// This event handler is used to prevent Expander to expand on click everywhere + selects the element on ListView.
        /// </summary>
        private void Control_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ProcessConfigurationView? selectedItem = FindConfigurationView(sender);
            if (selectedItem != null)
            {
                ViewModel!.SelectedView = selectedItem;
            }

            e.Handled = true;
        }


        private ProcessConfigurationView? FindConfigurationView(object control)
        {
            if (control is FrameworkElement element)
            {
                ProcessConfigurationView? selectedItem = element.DataContext as ProcessConfigurationView;
                if (selectedItem != null)
                {
                    return selectedItem;
                }

                return FindConfigurationView(element.Parent);
            }

            return null;
        }
    }
}
