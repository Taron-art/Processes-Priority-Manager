using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Affinity_manager.UserControls
{
    [ContentProperty(Name = nameof(Decorated))]
    public sealed partial class ControlWithHeader : UserControl
    {
        public ControlWithHeader()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(ControlWithHeader), new PropertyMetadata(null));

        public string? Header
        {
            get => (string?)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty DecoratedProperty = DependencyProperty.Register(nameof(Header), typeof(object), typeof(ControlWithHeader), new PropertyMetadata(null));

        public object? Decorated
        {
            get => (object?)GetValue(DecoratedProperty);
            set => SetValue(DecoratedProperty, value);
        }
    }
}
