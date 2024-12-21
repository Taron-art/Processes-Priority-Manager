using System;
using Microsoft.UI.Xaml.Data;
using Windows.UI.Text;

namespace Affinity_manager.XamlHelpers
{
    internal class BoolToFontStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((value is bool boolValue) && (targetType == typeof(TextDecorations)))
            {
                return boolValue ? TextDecorations.Underline : TextDecorations.None;
            }

            throw new NotSupportedException("Conversion is not supported");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
