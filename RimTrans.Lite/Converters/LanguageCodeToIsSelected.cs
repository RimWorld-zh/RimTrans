using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RimTrans.Lite.Converters
{
    public class LanguageCodeToIsSelected : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = value as string;

            if (param == null)
                return DependencyProperty.UnsetValue;

            return Equals(param, parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterString = parameter as string;

            if (parameterString == null || value.Equals(false))
                return DependencyProperty.UnsetValue;

            return parameter;
        }
    }
}
