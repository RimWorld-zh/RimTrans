using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RimTrans.Lite.Windows
{
    /// <summary>
    /// About.xaml 的交互逻辑
    /// </summary>
    public partial class About : UserControl
    {
        public About()
        {
            InitializeComponent();
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/duduluu/RimTrans/blob/master/LICENSE");
        }

        private void MIT_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/duduluu/RimTrans");
        }

        private void RimWorld_zh_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://RimWorld-zh.com/");
        }


    }
}
