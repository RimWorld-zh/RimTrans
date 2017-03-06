using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
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
            Process.Start("https://github.com/duduluu/RimTrans/tree/master/RimTrans.Lite/Resources/Localizations");
        }

        private void buttonAutoRWInstallDir_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RimWorldHelper.RimWorldInstallDir) || !Directory.Exists(RimWorldHelper.RimWorldInstallDir))
            {
                var dialog = new AwesomeDialog();
                if (Application.Current.Resources.Contains("Options.Message.DirectoryNoFound"))
                    dialog.Message = (string)Application.Current.FindResource("Options.Message.DirectoryNoFound");
                else
                    dialog.Message = "RimWorld install directory or Steam Workshop Mods Directory on found. This can be caused by several problems. You could custom in options.";
                dialog.ShowDialog(this);
            }
            else
            {
                UserSettings.All.RimWorldInstallDir = RimWorldHelper.RimWorldInstallDir;
            }
        }

        private void buttonAutoWSModsDir_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RimWorldHelper.WorkshopModsDir) || !Directory.Exists(RimWorldHelper.WorkshopModsDir))
            {
                var dialog = new AwesomeDialog();
                if (Application.Current.Resources.Contains("Options.Message.DirectoryNoFound"))
                    dialog.Message = (string)Application.Current.FindResource("Options.Message.DirectoryNoFound");
                else
                    dialog.Message = "RimWorld install directory or Steam Workshop Mods Directory on found. This can be caused by several problems. You could custom in options.";
                dialog.ShowDialog(this);
            }
            else
            {
                UserSettings.All.WorkshopModsDir = RimWorldHelper.WorkshopModsDir;
            }
        }

        private void buttonResetDoNotPrompt_Click(object sender, RoutedEventArgs e)
        {
            var all = UserSettings.All;
            all.ExtractDoNotPromptClean = false;

            var dialog = new AwesomeDialog();
            if (Application.Current.Resources.Contains("Options.ResetDoNotPrompt.Message"))
                dialog.Message = (string)Application.Current.FindResource("Options.ResetDoNotPrompt.Message");
            else
                dialog.Message = "All Do-Not-Prompt dialogs have been reset.";
            dialog.ShowDialog(this);
        }
    }
}
