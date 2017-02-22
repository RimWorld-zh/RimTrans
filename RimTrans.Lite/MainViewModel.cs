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
using System.Collections.ObjectModel;

using duduluu.MVVM;
using RimTrans.Lite.Dialog;
using RimTransLite.AwesomeControl;


namespace RimTrans.Lite
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Mods = new ObservableCollection<ModsListItem>();
        }

        public MainWindow View { get; set; }

        public ObservableCollection<ModsListItem> Mods { get; set; }

        private ModsListItem _selectedMod;
        public ModsListItem SelectedMod
        {
            get { return _selectedMod; }
            set
            {
                _selectedMod = value;
                OnPropertyChanged("SelectedMod");
            }
        }

        private LanguagesListItem _selectedLanguage;
        public LanguagesListItem SelecteLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged("SelecteLanguage");
            }
        }

        #region Mod Command

        // Can execute: has selected mod
        private bool CanExecuteHasSelectedMod(object parameter)
        {
            return !(_selectedMod == null);
        }

        // Add mod command
        private RelayCommand _addModCommand;
        public RelayCommand AddModCommand
        {
            get
            {
                return _addModCommand ?? (_addModCommand = new RelayCommand(ExecuteAddMod));
            }
        }
        private void ExecuteAddMod(object parameter)
        {
            var dlg = new AddModDialog();
            var result = dlg.ShowDialog(View);
            if (result == true)
            {
                var mod = dlg.ResultMod;
                Mods.Add(mod);
                SelectedMod = mod;
            }
        }

        // Show mod in Explorer command
        private RelayCommand _showModInExplorerCommand;
        public RelayCommand ShowModInExplorerCommand
        {
            get
            {
                return _showModInExplorerCommand ?? (_showModInExplorerCommand = new RelayCommand(ExecuteShowModInExplorer, CanExecuteHasSelectedMod));
            }
        }
        private void ExecuteShowModInExplorer(object parameter)
        {
            System.Diagnostics.Process.Start("explorer.exe", _selectedMod.ModPath);
        }

        // Remove mod command
        private RelayCommand _removeModCommand;
        public RelayCommand RemoveModCommand
        {
            get
            {
                return _removeModCommand ?? (_removeModCommand = new RelayCommand(ExecuteRemoveMod, CanExecuteHasSelectedMod));
            }
        }
        private void ExecuteRemoveMod(object parameter)
        {
            Mods.Remove(_selectedMod);
        }

        // Modify mod command

        #endregion

        #region Language Command

        // Can execute: has selected language
        private bool CanExecuteHasSelectedLanguage(object parameter)
        {
            return _selectedLanguage != null;
        }

        // Add language command
        private RelayCommand _addLanguageCommand;
        public RelayCommand AddLanguageCommand
        {
            get
            {
                return _addLanguageCommand ?? (_addLanguageCommand = new RelayCommand(ExecuteAddLanguage, CanExecuteHasSelectedMod));
            }
        }
        private void ExecuteAddLanguage(object parameter)
        {
            var dlg = new AddLanguageDialog();
            var result = dlg.ShowDialog(View);
            if (result == true)
            {
                foreach (LanguagesListItem lang in dlg.ResultLanguages)
                {
                    _selectedMod.Languages.Add(lang);
                }
            }
        }

        // Show language command
        private RelayCommand _showLanguageInExplorer;
        public RelayCommand ShowLanguageInExplorer
        {
            get
            {
                return _showLanguageInExplorer ?? (_showLanguageInExplorer = new RelayCommand(ExecuteShowLanguageInExplore, CanExecuteHasSelectedLanguage));
            }
        }
        private void ExecuteShowLanguageInExplore(object parameter)
        {
            if (_selectedLanguage.LanguagePath == null || _selectedLanguage.LanguagePath == string.Empty)
            {
                string path = System.IO.Path.Combine(_selectedMod.ModPath, "Languages", _selectedLanguage.LanguageName);
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            else
            {
                System.Diagnostics.Process.Start("explorer.exe", _selectedLanguage.LanguagePath);
            }
        }

        // Remove language command
        private RelayCommand _removeLanguageCommand;
        public RelayCommand RemoveLanguageCommand
        {
            get
            {
                return _removeLanguageCommand ?? (_removeLanguageCommand = new RelayCommand(ExecuteRemoveLanguage, CanExecuteHasSelectedLanguage));
            }
        }
        private void ExecuteRemoveLanguage(object parameter)
        {
            _selectedMod.Languages.Remove(_selectedLanguage);
        }

        #endregion

        #region Build

        private RelayCommand _buildCommand;
        public RelayCommand BuildCommand
        {
            get
            {
                return _buildCommand ?? (_buildCommand = new RelayCommand(ExecuteBuild, CanExecuteHasSelectedMod));
            }
        }
        private void ExecuteBuild(object parameter)
        {
            var dlg = new Dialog.BuildDialog(_selectedMod);
            var result = dlg.ShowDialog(View);
        }

        #endregion

        #region Config

        private RelayCommand _configCommand;
        public RelayCommand ConfigCommand
        {
            get
            {
                return _configCommand ?? (_configCommand = new RelayCommand(ExecuteConfig));
            }
        }
        private void ExecuteConfig(object parameter)
        {
            var dlg = new ConfigDialog();
            dlg.ShowDialog(View);
        }

        #endregion



    }
}
