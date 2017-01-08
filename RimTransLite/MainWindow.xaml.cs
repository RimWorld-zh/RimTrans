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
using FontAwesome.WPF;
using RimTransLite.AwesomeControl;
using RimTransLite.AwesomeDialog;

namespace RimTransLite
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonAddMod_Click(object sender, RoutedEventArgs e)
        {
            DialogAddMod dlg = new DialogAddMod();
            var dlgResult = dlg.ShowDialog();
            if (dlgResult == true)
            {
                modsListBox.AddMod(dlg.SelectedMod);
            }
        }

        private void buttonAddLanguage_Click(object sender, RoutedEventArgs e)
        {
            languagesListBox.AddLanguage(AwesomeControl.LanguagesListItem.LanguageCategory.Internal, "ChineseSimplified", @"D:\Game\RimWorld\Mods\Core\Languages\ChineseSimplified");
        }

        private void languagesListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            languagesListBox.SelectedItem = null;
        }
    }
}
