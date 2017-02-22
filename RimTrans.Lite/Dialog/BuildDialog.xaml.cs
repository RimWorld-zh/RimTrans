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

using FontAwesome.WPF;
using duduluu.MVVM;
using RimTransLite.AwesomeControl;
using RimTransLite.Options;

namespace RimTrans.Lite.Dialog
{
    /// <summary>
    /// DialogBuild.xaml 的交互逻辑
    /// </summary>
    public partial class BuildDialog : Window
    {
        public BuildDialog(ModsListItem selectedMod)
        {
            InitializeComponent();
            var vm = new BuildViewModel();
            vm.SelectedMod = selectedMod;
            vm.View = this;
            //vm.Outputter = new TextBoxOutputter(logOutput);
            this.DataContext = vm;
        }
    }
}
