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
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            var mods = new ObservableCollection<ModListBoxItem>();
            string projectsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RimTrans", "Projects");
            if (Directory.Exists(projectsDir))
            {
                foreach (string projectFile in Directory.GetFiles(projectsDir))
                {
                    ModListBoxItem modItem = null;
                    try
                    {
                        modItem = ModListBoxItem.Load(projectFile);
                    }
                    catch (Exception)
                    {
                    }
                    if (modItem == null)
                    {
                        try
                        {
                            File.Delete(projectFile);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        mods.Add(modItem);
                    }
                }
            }
            Mods = mods;
        }

        public MainWindow View { get; set; }

        public ObservableCollection<ModListBoxItem> Mods { get; set; }


        private ModListBoxItem _selectedMod;
        public ModListBoxItem SelectedMod
        {
            get { return _selectedMod; }
            set
            {
                _selectedMod = value;
                OnPropertyChanged("SelectedMod");
            }
        }

        private LanguageListBoxItem _selectedLanguage;
        public LanguageListBoxItem SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged("SelectedLanguage");
            }
        }

        #region Command

        #region CanExecute Methods
        
        private bool CanExecuteHasSelectedMod(object parameter)
        {
            return _selectedMod != null;
        }

        private bool CanExecuteHasSelectedLanguage(object parameter)
        {
            return _selectedLanguage != null;
        }

        #endregion

        #region Mod Command

        // Add Mod
        private RelayCommand _commandAddMod;
        public RelayCommand CommandAddMod
        {
            get { return _commandAddMod ?? (_commandAddMod = new RelayCommand(ExecuteAddMod)); }
        }
        private void ExecuteAddMod(object parameter)
        {
            var window = new AddModWindow();
            var dialogResult = window.ShowDialog(View);
            if (dialogResult == true)
            {
                var modItem = window.Result;
                modItem.InitialProjectFileName();
                string projectsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RimTrans", "Projects");
                if (!Directory.Exists(projectsDir))
                {
                    Directory.CreateDirectory(projectsDir);
                }
                var projectFile = Path.Combine(projectsDir, modItem.ProjectFileName);
                try
                {
                    modItem.Save(projectFile);
                }
                catch (Exception)
                {
                }
                Mods.Add(window.Result);
            }
        }

        // Remove Mod

        private void ExecuteRemoveMod(object parameter)
        {

        }

        #endregion



        // Add Language
        private RelayCommand _commandAddLanguage;
        public RelayCommand CommandAddLanguage
        {
            get { return _commandAddLanguage ?? (_commandAddLanguage = new RelayCommand(ExecuteAddLanguage, CanExecuteHasSelectedMod)); }
        }
        private void ExecuteAddLanguage(object parameter)
        {

        }

        // Extract
        private RelayCommand _commandExtract;
        public RelayCommand CommandExtract
        {
            get { return _commandExtract ?? (_commandExtract = new RelayCommand(ExecuteExtract, CanExecuteHasSelectedMod)); }
        }
        private void ExecuteExtract(object parameter)
        {
            string projectsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RimTrans", "Projects");
            string projectFile = Path.Combine(projectsDir, _selectedMod.ProjectFileName);
            string corePath = RimWorldHelper.GetCorePath();
            var selectedMod = _selectedMod;
            SelectedMod = null;
            selectedMod.Save(projectFile);

            string arguments = $"\"-p:{projectFile}\" -Core:\"{corePath}\"";

            var window = new ExtractWindow();
            window.SelectedMod = selectedMod;
            var dialogResult = window.ShowDialog(View);

            if (dialogResult == true)
            {
                if (window.IsCleanMode) arguments += " -Clean";
                Process.Start("Trans.exe", arguments);
            }

            SelectedMod = selectedMod;
        }

        // Editor (TODO)

        // Options
        private RelayCommand _commandOptions;
        public RelayCommand CommandOptions
        {
            get { return _commandOptions ?? (_commandOptions = new RelayCommand(ExecuteOptions)); }
        }
        private void ExecuteOptions(object parameter)
        {
            var window = new OptionsWindow();
            var dialogResult = window.ShowDialog(View);
        }

        #endregion

        public void SaveProjects()
        {
            string projectsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RimTrans", "Projects");
            if (!Directory.Exists(projectsDir))
            {
                Directory.CreateDirectory(projectsDir);
            }
            foreach (ModListBoxItem modItem in Mods)
            {
                if (string.IsNullOrWhiteSpace(modItem.ProjectFileName))
                {
                    modItem.InitialProjectFileName();
                }
                string path = Path.Combine(projectsDir, modItem.ProjectFileName);
                try
                {
                    modItem.Save(path);
                }
                catch (Exception)
                {
                }
            }
        }

    }
}
