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
    public class LanguagesListItem : ListItem
    {
        public enum LanguageCategory { Internal, Custom }

        public LanguagesListItem(LanguageCategory category, string name, string path = null)
        {
            this.Category = category;
            this.LanguageName = name;
            this.LanguagePath = path;
            if (path == name || path == string.Empty)
            {
                this.LanguageNameNative = name;
            }
            else
            {
                try
                {
                    XDocument languageInfoDoc = XDocument.Load(System.IO.Path.Combine(path, "LanguageInfo.xml"));
                    this.LanguageNameNative = languageInfoDoc.Root.Element("friendlyNameNative").Value;
                }
                catch (Exception)
                {
                    this.LanguageNameNative = name;
                }
            }
        }


        public LanguageCategory Category
        {
            get { return (LanguageCategory)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Category.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(LanguageCategory), typeof(LanguagesListItem), new PropertyMetadata(LanguageCategory.Internal));


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
            DependencyProperty.Register("LanguagePath", typeof(string), typeof(LanguagesListItem));



    }

    /// <summary>
    /// LiteLanguagesListBox.xaml 的交互逻辑
    /// </summary>
    public partial class LanguagesListBox : ListBox
    {
        public LanguagesListBox()
        {
            InitializeComponent();
            languages = new ObservableCollection<LanguagesListItem>();
            this.ItemsSource = languages;
            this.DataContext = this;
        }

        ObservableCollection<LanguagesListItem> languages;

        public void AddLanguage(LanguagesListItem.LanguageCategory category, string name, string path = null)
        {
            languages.Add(new LanguagesListItem(category, name, path));
        }
    }
}
