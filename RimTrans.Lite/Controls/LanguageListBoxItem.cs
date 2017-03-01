using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FontAwesome.WPF;
using RimTrans.Lite.Util;

namespace RimTrans.Lite.Controls
{
    public class LanguageListBoxItem : ListBoxItem
    {

        /// <summary>
        /// The directory path of the language.
        /// </summary>
        public string LangPath
        {
            get { return (string)GetValue(LangPathProperty); }
            set { SetValue(LangPathProperty, value); }
        }
        public static readonly DependencyProperty LangPathProperty =
            DependencyProperty.Register("LangPath", typeof(string), typeof(LanguageListBoxItem));

        
        /// <summary>
        /// Which language this actually is. Importan.
        /// </summary>
        [Bindable(true), Category("Common"), Description("The text of the button.")]
        public string RealName
        {
            get { return (string)GetValue(RealNameProperty); }
            set { SetValue(RealNameProperty, value); }
        }
        public static readonly DependencyProperty RealNameProperty =
            DependencyProperty.Register("RealName", typeof(string), typeof(LanguageListBoxItem));

        
        [Bindable(true), Category("Common"), Description("The text of the button.")]
        public string NativeName
        {
            get { return (string)GetValue(NativeNameProperty); }
            set { SetValue(NativeNameProperty, value); }
        }
        public static readonly DependencyProperty NativeNameProperty =
            DependencyProperty.Register("NativeName", typeof(string), typeof(LanguageListBoxItem));

        
        [Bindable(true), Category("Common"), Description("The text of the button.")]
        public bool? IsCustom
        {
            get { return (bool?)GetValue(IsCustomProperty); }
            set { SetValue(IsCustomProperty, value); }
        }
        public static readonly DependencyProperty IsCustomProperty =
            DependencyProperty.Register("IsCustom", typeof(bool?), typeof(LanguageListBoxItem));


        [Bindable(true), Category("Common"), Description("The text of the button.")]
        public string CustomPath
        {
            get { return (string)GetValue(CustomPathProperty); }
            set { SetValue(CustomPathProperty, value); }
        }
        public static readonly DependencyProperty CustomPathProperty =
            DependencyProperty.Register("CustomPath", typeof(string), typeof(LanguageListBoxItem));


        [Bindable(true), Category("Common"), Description("The text of the button.")]
        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool?), typeof(LanguageListBoxItem));


    }
}
