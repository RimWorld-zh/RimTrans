using System;
using System.Collections.Generic;
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
using System.Xml;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using FontAwesome.WPF;

namespace RimTransLite.AwesomeControl
{
    #region LanguagesListItem Class

    public class LanguagesListItem : ListItem
    {
        public LanguagesListItem(string name, string nameNative, string path = null)
        {
            this.LanguageName = name;
            this.LanguageNameNative = nameNative; 
            this.LanguagePath = path;
        }

        public string LanguageName
        {
            get { return (string)GetValue(LanguageNameProperty); }
            set { SetValue(LanguageNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LanguageName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LanguageNameProperty =
            DependencyProperty.Register("LanguageName", typeof(string), typeof(LanguagesListItem), new PropertyMetadata("Lang"));


        public string LanguageNameNative
        {
            get { return (string)GetValue(LanguageNameNativeProperty); }
            set { SetValue(LanguageNameNativeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LanguageNameNative.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LanguageNameNativeProperty =
            DependencyProperty.Register("LanguageNameNative", typeof(string), typeof(LanguagesListItem), new PropertyMetadata("Lang"));


        public string LanguagePath
        {
            get { return (string)GetValue(LanguagePathProperty); }
            set { SetValue(LanguagePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LanguagePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LanguagePathProperty =
            DependencyProperty.Register("LanguagePath", typeof(string), typeof(LanguagesListItem), new PropertyMetadata(string.Empty));


        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(LanguagesListItem), new PropertyMetadata(false));

        
        /// <summary>
        /// RimWorld build-in basic languages
        /// </summary>
        public static ObservableCollection<LanguagesListItem> PrimaryLanguages()
        {
            ObservableCollection<LanguagesListItem> languages = new ObservableCollection<LanguagesListItem>();
            languages.Add(new LanguagesListItem("ChineseSimplified", "简体中文"));
            languages.Add(new LanguagesListItem("ChineseTraditional", "繁體中文"));
            languages.Add(new LanguagesListItem("Czech", "Čeština"));
            languages.Add(new LanguagesListItem("Danish", "Dansk"));
            languages.Add(new LanguagesListItem("Dutch", "Nederlands"));
            languages.Add(new LanguagesListItem("English", "English"));
            languages.Add(new LanguagesListItem("Estonian", "Eesti"));
            languages.Add(new LanguagesListItem("Finnish", "Suomi"));
            languages.Add(new LanguagesListItem("French", "Français"));
            languages.Add(new LanguagesListItem("German", "Deutsch"));
            languages.Add(new LanguagesListItem("Hungarian", "Magyar"));
            languages.Add(new LanguagesListItem("Italian", "Italiano."));
            languages.Add(new LanguagesListItem("Japanese", "日本語"));
            languages.Add(new LanguagesListItem("Korean", "한국어"));
            languages.Add(new LanguagesListItem("Norwegian", "Norsk Bokmål"));
            languages.Add(new LanguagesListItem("Polish", "Polski"));
            languages.Add(new LanguagesListItem("Portuguese", "Português"));
            languages.Add(new LanguagesListItem("PortugueseBrazilian", "Português Brasileiro"));
            languages.Add(new LanguagesListItem("Romanian", "Română"));
            languages.Add(new LanguagesListItem("Russian", "Русский"));
            languages.Add(new LanguagesListItem("Slovak", "Slovenčina"));
            languages.Add(new LanguagesListItem("Spanish", "Español(Castellano)"));
            languages.Add(new LanguagesListItem("SpanishLatin", "Español(Latinoamérica)"));
            languages.Add(new LanguagesListItem("Swedish", "Svenska"));
            languages.Add(new LanguagesListItem("Turkish", "Türkçe"));
            languages.Add(new LanguagesListItem("Ukrainian", "Українська"));
            return languages;
        }
    }
    
    #endregion

    /// <summary>
    /// LiteLanguagesListBox.xaml 的交互逻辑
    /// </summary>
    public partial class LanguagesListBox : ListBox
    {
        public LanguagesListBox()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void ListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedItem = null;
        }
    }
}
