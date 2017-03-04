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
using System.Windows.Shapes;
using duduluu.MVVM;
using RimTrans.Lite.Controls.Dialogs;

namespace RimTrans.Lite.Windows
{
    /// <summary>
    /// AddLanguageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddLanguageWindow : Window
    {
        public AddLanguageWindow()
        {
            InitializeComponent();
        }

        public IEnumerable<string> Result { get; set; }

        private void buttonConfirm_Click(object sender, RoutedEventArgs e)
        {
            Result = SelectedLanguages();
            DialogResult = true;
        }

        private IEnumerable<string> SelectedLanguages()
        {
            foreach (var child in languageGroup.Children)
            {
                var subLanguageGroup = child as StackPanel;
                foreach (var grandchild in subLanguageGroup.Children)
                {
                    var checkBoxLanguage = grandchild as CheckBox;
                    if (checkBoxLanguage.IsChecked == true)
                    {
                        //var langItem = new LanguageListBoxItem();
                        //string content = checkBoxLanguage.Content as string;
                        //int splitIndex = content.IndexOf('(');
                        //langItem.RealName = content.Substring(0, splitIndex - 1);
                        //langItem.NativeName = content.Substring(splitIndex + 2, content.Length - splitIndex - 4);
                        //langItem.IsChecked = true;
                        //langItem.IsChecked = false;
                        //yield return langItem;
                        yield return checkBoxLanguage.Content as string;
                    }
                }
            }
        }
    }
}
