using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using FontAwesome.WPF;

namespace RimTrans.Lite.Converters
{
    public class IsCheckedToCircleIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? isChecked = (bool?)value;
            switch (isChecked)
            {
                case null:
                    return FontAwesomeIcon.Circle;
                case true:
                    return FontAwesomeIcon.DotCircleOutline;
                case false:
                default:
                    return FontAwesomeIcon.CircleOutline;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FontAwesomeIcon icon = (FontAwesomeIcon)value;
            switch (icon)
            {
                case FontAwesomeIcon.Circle:
                    return null;
                case FontAwesomeIcon.DotCircleOutline:
                    return true;
                case FontAwesomeIcon.CircleOutline:
                default:
                    return false;
            }
        }
    }
}
