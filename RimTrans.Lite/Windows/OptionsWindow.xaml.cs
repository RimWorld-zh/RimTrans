using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using RimTrans.Lite.Util;

namespace RimTrans.Lite.Windows
{
    /// <summary>
    /// WindowOptions.xaml 的交互逻辑
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
        }

        private void Options_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserSettings.Save();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LocalizationHelper.SelectLanguage(UserSettings.All.LanguageCode);
        }

        private void HelpTranslateRimTrans_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("");
        }
    }
}
