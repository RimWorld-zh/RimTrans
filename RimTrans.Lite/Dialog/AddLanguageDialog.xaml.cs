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
using System.Collections.ObjectModel;

using duduluu.MVVM;
using RimTransLite.AwesomeControl;

namespace RimTrans.Lite.Dialog
{
    /// <summary>
    /// DialogAddLanguage.xaml 的交互逻辑
    /// </summary>
    public partial class AddLanguageDialog : Window
    {
        public AddLanguageDialog()
        {
            InitializeComponent();
            var vm = new AddLanguageViewModel();
            vm.View = this;
            this.DataContext = vm;
        }
        
        public IEnumerable<LanguagesListItem> ResultLanguages { get; set; }
    }
}
