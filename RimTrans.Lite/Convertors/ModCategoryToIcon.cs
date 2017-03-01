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

namespace RimTrans.Lite.Convertors
{
    class ModCategoryToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ModCategory modCategory = (ModCategory)value;
            switch (modCategory)
            {
                case ModCategory.Custom:
                    return FontAwesomeIcon.HddOutline;
                case ModCategory.Internal:
                    return FontAwesomeIcon.FolderOutline;
                case ModCategory.Workshop:
                    return FontAwesomeIcon.Steam;
                default:
                    return FontAwesomeIcon.HddOutline;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FontAwesomeIcon icon = (FontAwesomeIcon)value;
            switch (icon)
            {
                case FontAwesomeIcon.HddOutline:
                    return ModCategory.Custom;
                case FontAwesomeIcon.FolderOutline:
                    return ModCategory.Internal;
                case FontAwesomeIcon.Steam:
                    return ModCategory.Workshop;
                default:
                    return ModCategory.Custom;
            }
        }
    }
}
