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
using RimTrans.Lite.Controls;
using RimTrans.Lite.Controls.Dialogs;
using RimTrans.Lite.Util;

namespace RimTrans.Lite.Windows
{
    /// <summary>
    /// ExtractWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExtractWindow : Window
    {
        public ExtractWindow()
        {
            InitializeComponent();
        }



        public ModListBoxItem SelectedMod
        {
            get { return (ModListBoxItem)GetValue(SelectedModProperty); }
            set { SetValue(SelectedModProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedMod.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedModProperty =
            DependencyProperty.Register("SelectedMod", typeof(ModListBoxItem), typeof(ExtractWindow));





        public bool IsCleanMode
        {
            get { return (bool)GetValue(IsCleanModeProperty); }
            set { SetValue(IsCleanModeProperty, value); }
        }
        public static readonly DependencyProperty IsCleanModeProperty =
            DependencyProperty.Register("IsCleanMode", typeof(bool), typeof(ExtractWindow), new PropertyMetadata(false));

        private void buttonExtract_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void checkBoxCleanMode_Checked(object sender, RoutedEventArgs e)
        {
            if (!UserSettings.All.ExtractDoNotPromptClean)
            {
                var dialog = new AwesomeDialog();
                dialog.Title = checkBoxCleanMode.Content as string;
                dialog.Message = checkBoxCleanMode.ToolTip as string;
                dialog.AwesomeIcon = FontAwesome.WPF.FontAwesomeIcon.Warning;
                dialog.ButtonTags = ButtonTag.Cancel | ButtonTag.Confirm;
                dialog.ShowDoNotPromptCheckBox = true;
                dialog.ShowDialog(this);
                UserSettings.All.ExtractDoNotPromptClean = dialog.DoNotPromptResult;
                if (dialog.Result != ButtonTag.Confirm)
                {
                    checkBoxCleanMode.IsChecked = false;
                }
            }
        }

        private void buttonSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (LanguageListBoxItem langItem in SelectedMod.Languages)
            {
                langItem.IsChecked = true;
            }
        }

        private void buttonSelectInvert_Click(object sender, RoutedEventArgs e)
        {
            foreach (LanguageListBoxItem langItem in SelectedMod.Languages)
            {
                langItem.IsChecked = !langItem.IsChecked;
            }
        }
    }
}
