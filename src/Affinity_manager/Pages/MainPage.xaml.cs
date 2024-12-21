using System;
using Affinity_manager.ViewWrappers;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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

        public MainPageViewModel ViewModel { get; } = new MainPageViewModel();

        private void KeyboardAccelerator_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            ViewModel.AddCommand.Execute(null);
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
            //var selectorControl = new AffinitySelector();
            //selectorControl.ProcessAffinity = (AffinityView)((Button)sender).DataContext;

            //var affinityDialog = new ContentDialog
            //{
            //    Title = "Bla",
            //    XamlRoot = XamlRoot,
            //    Content = selectorControl,
            //    PrimaryButtonText = "OK",
            //    CloseButtonText = "Cancel"
            //};
            ////ContentDialog dialog = new ContentDialog();

            ////// XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            ////dialog.XamlRoot = this.XamlRoot;
            ////dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            ////dialog.Title = "Save your work?";
            ////dialog.PrimaryButtonText = "Save";
            ////dialog.SecondaryButtonText = "Don't Save";
            ////dialog.CloseButtonText = "Cancel";
            ////dialog.DefaultButton = ContentDialogButton.Primary;
            ////dialog.ShowAsync();

            ////editAffinityDialog.DataContext = (AffinityView)((Button)sender).DataContext;
            ////editAffinityDialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            ////ContentDialogResult result = await editAffinityDialog.ShowAsync();

            //var result = await affinityDialog.ShowAsync();
            //if (result == ContentDialogResult.Primary)
            //{
            //    selectorControl.ProcessAffinity.ApplyChanges();
            //}
            //else
            //{
            //    selectorControl.ProcessAffinity.CancelChanges();
            //}
            AffinitySelectorDialog dialog = new AffinitySelectorDialog((AffinityView)((Button)sender).DataContext);
            dialog.XamlRoot = XamlRoot;
            await dialog.ShowAsync();
        }

        private async void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (!ViewModel.IsInterfaceVisible)
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
            await ViewModel.Reload();
        }
    }
}
