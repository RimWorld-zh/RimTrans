using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using FontAwesome.WPF;

namespace RimTrans.Lite.Convertors
{
    public class IsCheckedToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? isChecked = (bool?)value;
            switch (isChecked)
            {
                case null:
                    return FontAwesomeIcon.Square;
                case true:
                    return FontAwesomeIcon.CheckSquare;
                case false:
                default:
                    return FontAwesomeIcon.SquareOutline;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FontAwesomeIcon icon = (FontAwesomeIcon)value;
            switch (icon)
            {
                case FontAwesomeIcon.Square:
                    return null;
                case FontAwesomeIcon.CheckSquare:
                    return true;
                case FontAwesomeIcon.SquareOutline:
                default:
                    return false;
            }
        }
    }
}
