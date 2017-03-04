using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using RimTrans.Lite.Controls.Dialogs;

namespace RimTrans.Lite.Converters
{
    class ButtonTagsToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ButtonTag v = (ButtonTag)value;
            ButtonTag p;
            if (Enum.TryParse(parameter as string, out p))
            {
                return (v & p) == p ? Visibility.Visible : Visibility.Collapsed;

            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
