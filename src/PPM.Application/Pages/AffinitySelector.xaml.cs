using Affinity_manager.ViewWrappers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Affinity_manager.Pages
{
    public sealed partial class AffinitySelector : UserControl
    {
        public AffinitySelector()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty ProcessAffinityProperty =
            DependencyProperty.Register(
                nameof(ProcessAffinity),
                typeof(ProcessConfigurationView),
                typeof(AffinitySelector),
                null);

        public AffinityView ProcessAffinity
        {
            get
            {
                return (AffinityView)GetValue(ProcessAffinityProperty);
            }
            set
            {
                SetValue(ProcessAffinityProperty, value);
            }
        }
    }
}
