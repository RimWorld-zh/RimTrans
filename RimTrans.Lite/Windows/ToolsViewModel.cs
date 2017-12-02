using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using duduluu.MVVM;
using RimTrans.Lite.Controls;
using RimTrans.Lite.Util;

namespace RimTrans.Lite.Windows
{
    public class ToolsViewModel : ViewModelBase
    {
        public ToolsViewModel()
        {

        }

        public ToolsWindow View { get; set; }

        // TemplateExport
        private bool CanExecuteExport(object parameter) {
            return !string.IsNullOrWhiteSpace(UserSettings.All.DefTemplateOutputPath);
        }
        private RelayCommand _commandTemplateExport;
        public RelayCommand CommandTemplateExport {
            get { return _commandTemplateExport ?? (_commandTemplateExport = new RelayCommand(ExecuteTemplateExport, CanExecuteExport)); }
        }
        private void ExecuteTemplateExport(object parameter) {
            string defsPath = Path.Combine(UserSettings.All.RimWorldInstallDir, "Mods", "Core", "Defs");
            string outputPath = UserSettings.All.DefTemplateOutputPath;
            string arguments = $"\"{defsPath}\" \"{outputPath}\"";
            if (UserSettings.All.DefTemplateOpenFolder) {
                Process.Start("explorer.exe", $"\"{outputPath}\"");
            }
            Process.Start("TemplateExporter.exe", arguments);
        }
    }
}
