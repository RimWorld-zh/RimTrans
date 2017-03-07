using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Reflection;
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
            Assembly asm = Assembly.GetExecutingAssembly();
            title.Text = asm.GetCustomAttribute<AssemblyTitleAttribute>().Title;
            version.Text = "Version " + asm.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            copyright.Text = asm.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
            description.Text = asm.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
