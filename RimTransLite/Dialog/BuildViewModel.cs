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
using System.Windows.Threading;
using System.IO;
using System.Collections.ObjectModel;

using FontAwesome.WPF;
using duduluu.MVVM;
using RimTrans.Builder;
using RimTransLite.AwesomeControl;
using RimTransLite.Options;

namespace RimTrans.Lite.Dialog
{
    class BuildViewModel : ViewModelBase
    {
        public BuildViewModel()
        {
            IsModWithCoreMode = true;
            Logs = new ObservableCollection<string>();
        }
        
        public BuildDialog View { get; set; }
        
        public ModsListItem SelectedMod { get; set; }

        public ObservableCollection<string> Logs { get; set; }

        //public TextBoxOutputter Outputter { get; set; }

        private bool _isCoreMode;
        public bool IsCoreMode
        {
            get { return _isCoreMode; }
            set
            {
                _isCoreMode = value;
                if (value)
                {
                    IsModWithCoreMode = false;
                    IsModWithoutCoreMode = false;
                }
                OnPropertyChanged("IsCoreMode");
            }
        }

        private bool _isModWithCoreMode;
        public bool IsModWithCoreMode
        {
            get { return _isModWithCoreMode; }
            set
            {
                _isModWithCoreMode = value;
                if (value)
                {
                    IsCoreMode = false;
                    IsModWithoutCoreMode = false;
                }
                OnPropertyChanged("IsModWithCoreMode");
            }
        }

        private bool _isModWithoutCoreMode;
        public bool IsModWithoutCoreMode
        {
            get { return _isModWithoutCoreMode; }
            set
            {
                _isModWithoutCoreMode = value;
                if (value)
                {
                    IsCoreMode = false;
                    IsModWithCoreMode = false;
                }
                OnPropertyChanged("IsModWithoutCoreMode");
            }
        }

        private bool _isFreshBuild;
        public bool IsFreshBuild
        {
            get { return _isFreshBuild; }
            set
            {
                _isFreshBuild = value;
                OnPropertyChanged("IsFreshBuild");
            }
        }

        private string _logText;
        public string LogText
        {
            get { return _logText; }
            set
            {
                _logText = value;
                OnPropertyChanged("LogText");
            }
        }

        // Command Build
        private RelayCommand _buildCommand;
        public RelayCommand BuildCommand
        {
            get
            {
                return _buildCommand ?? (_buildCommand = new RelayCommand(ExecuteBuild, CanExecuteBuild));
            }
        }
        private void ExecuteBuild(object parameter)
        {
            //View.logOutput.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    View.logOutput.AppendText("======== Begin ========");
            //}));
            //Console.WriteLine("======== Begin ========");
            Logs.Clear();
            Logs.Add("======== Begin ========");
            TransLog.MessageEventHandler += TransLog_MessageEventHandler;

            ModInfo modInfo = new ModInfo(SelectedMod.ModPath);
            List<LanguageInfo> languageInfos = new List<LanguageInfo>();
            foreach (LanguagesListItem lang in SelectedMod.Languages)
            {
                languageInfos.Add(new LanguageInfo(lang.LanguageName, lang.LanguageNameNative, lang.LanguagePath));
            }
            if (_isCoreMode)
            {
                ModData modData = new ModData(modInfo, languageInfos, true);
                modData.BuildLanguageData(_isFreshBuild);
            }
            else if (_isModWithCoreMode)
            {
                ModData coreData = new ModData(new ModInfo(LiteConfigs.PathCore), languageInfos, true);
                ModData modData = new ModData(modInfo, languageInfos, false, coreData);
                modData.BuildLanguageData(_isFreshBuild);
            }
            else if (_isModWithoutCoreMode)
            {
                ModData modData = new ModData(modInfo, languageInfos, false);
                modData.BuildLanguageData(_isFreshBuild);
            }

            TransLog.MessageEventHandler -= TransLog_MessageEventHandler;
            Logs.Add("======== Begin ========");

            //View.logOutput.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    View.logOutput.AppendText("======== End ========");
            //}));
            //Console.WriteLine("======== End ========");
        }
        private bool CanExecuteBuild(object parameter)
        {
            var selectedLanguages = from lang in SelectedMod.Languages
                                    where lang.IsChecked
                                    select lang;
            return selectedLanguages.Count() > 0;
        }

        private void TransLog_MessageEventHandler(object sender, TransLog.MessageArgs e)
        {
            string text = string.Empty;
            switch (e.Type)
            {
                case TransLog.Type.Message:
                    //LogText += "MESSAGE: ";
                    break;
                case TransLog.Type.Warning:
                    text += "WARNING: ";
                    break;
                case TransLog.Type.Error:
                    text += "ERROR: ";
                    break;
                default:
                    break;
            }
            text += e.Title + "\n";
            if (e.Detail != string.Empty && e.Detail != null)
            {
                text += e.Detail + "\n";
            }
            text += "\n";

            Logs.Add(text);
            //View.logOutput.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    View.logOutput.AppendText(text);
            //}));
        }
    }
}
