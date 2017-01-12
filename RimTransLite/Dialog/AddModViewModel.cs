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
using System.IO;
using System.Collections.ObjectModel;

using FontAwesome.WPF;
using duduluu.MVVM;
using RimTransLite.AwesomeControl;
using RimTransLite.Options;

namespace RimTrans.Lite.Dialog
{
    class AddModViewModel : ViewModelBase
    {
        public AddModViewModel()
        {
            Mods = new ObservableCollection<ModsListItem>();
            Languages = LanguagesListItem.PrimaryLanguages();
        }

        public AddModDialog View { get; set; }

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

        public ObservableCollection<LanguagesListItem> Languages { get; set; }

        // Internal Mods
        private RelayCommand _loadInternalModsCommand;
        public RelayCommand LoadInternalModsCommand
        {
            get
            {
                return _loadInternalModsCommand ?? (_loadInternalModsCommand = new RelayCommand(ExecuteLoadInternalMods));
            }
        }
        private void ExecuteLoadInternalMods(object parameter)
        {
            DirectoryInfo dir = new DirectoryInfo(LiteConfigs.PathInternalMods);
            if (dir.Exists)
            {
                Mods.Clear();
                try
                {
                    foreach (DirectoryInfo modDir in dir.GetDirectories())
                    {
                        Mods.Add(new ModsListItem(ModsListItem.Category.Internal, modDir.FullName, modDir.Name));
                    }
                }
                catch (Exception)
                {
                    Mods.Clear();
                }
            }
        }

        // Workshop Mods
        private RelayCommand _loadWorkshopModsCommand;
        public RelayCommand LoadWorkshopModsCommand
        {
            get
            {
                return _loadWorkshopModsCommand ?? (_loadWorkshopModsCommand = new RelayCommand(ExecuteLoadWorkshopMods));
            }
        }
        private void ExecuteLoadWorkshopMods(object parameter)
        {
            DirectoryInfo dir = new DirectoryInfo(LiteConfigs.PathWorkshopMods);
            if (dir.Exists)
            {
                Mods.Clear();
                try
                {
                    foreach (DirectoryInfo modDir in dir.GetDirectories())
                    {
                        Mods.Add(new ModsListItem(ModsListItem.Category.Workshop, modDir.FullName, modDir.Name));
                    }
                }
                catch (Exception)
                {
                    Mods.Clear();
                }
            }
        }

        // Custom Mod
        private RelayCommand _loadCustomModCommand;
        public RelayCommand LoadCustomModCommand
        {
            get
            {
                return _loadCustomModCommand ?? (_loadCustomModCommand = new RelayCommand(ExecuteLoadCustomMod));
            }
        }
        private void ExecuteLoadCustomMod(object parameter)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Mods.Clear();
                var customMod = new ModsListItem(ModsListItem.Category.Custom, dlg.SelectedPath);
                Mods.Add(customMod);
                SelectedMod = customMod;
            }
        }

        // Command Confrim
        private RelayCommand _confirmCommand;
        public RelayCommand ConfirmCommand
        {
            get
            {
                return _confirmCommand ?? (_confirmCommand = new RelayCommand(ExecuteConfirm, CanExecuteConfirm));
            }
        }
        private void ExecuteConfirm(object parameter)
        {
            foreach (LanguagesListItem lang in Languages)
            {
                if (lang.IsChecked)
                {
                    _selectedMod.Languages.Add(lang);
                }
            }
            View.ResultMod = _selectedMod;
            View.DialogResult = true;
        }
        private bool CanExecuteConfirm(object parameter)
        {
            return _selectedMod != null;
        }
    }
}
