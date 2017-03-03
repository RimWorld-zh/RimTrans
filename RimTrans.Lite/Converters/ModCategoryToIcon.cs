using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using FontAwesome.WPF;
using RimTrans.Lite.Util;

namespace RimTrans.Lite.Converters
{
    class ModCategoryToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var modCategory = value as ModCategory?;
            switch (modCategory)
            {
                case ModCategory.Internal:
                    return FontAwesomeIcon.FolderOutline;
                case ModCategory.Workshop:
                    return FontAwesomeIcon.Steam;
                case ModCategory.Custom:
                default:
                    return FontAwesomeIcon.HddOutline;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var icon = value as FontAwesomeIcon?;
            switch (icon)
            {
                case FontAwesomeIcon.FolderOutline:
                    return ModCategory.Internal;
                case FontAwesomeIcon.Steam:
                    return ModCategory.Workshop;
                case FontAwesomeIcon.HddOutline:
                default:
                    return ModCategory.Custom;
            }
        }
    }
}
