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
            this._projectsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RimTrans", "Projects");
            var mods = new ObservableCollection<ModListBoxItem>();
            if (Directory.Exists(_projectsDir))
            {
                foreach (string projectFile in Directory.GetFiles(_projectsDir))
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

        private string _projectsDir;

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

		private bool CanExecuteHasMods(object parameter) {
			return Mods.Count > 0;
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
                if (!Directory.Exists(_projectsDir))
                {
                    Directory.CreateDirectory(_projectsDir);
                }
                var projectFile = Path.Combine(_projectsDir, modItem.ProjectFileName);
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
        private RelayCommand _commandRemoveMod;
        public RelayCommand CommandRemoveMod
        {
            get { return _commandRemoveMod ?? (_commandRemoveMod = new RelayCommand(ExecuteRemoveMod, CanExecuteHasSelectedMod)); }
        }
        private void ExecuteRemoveMod(object parameter)
        {
            string projectFile = Path.Combine(_projectsDir, _selectedMod.ProjectFileName);
            Mods.Remove(_selectedMod);
            try
            {
                File.Delete(projectFile);
            }
            catch (Exception)
            {
            }
        }

		// Remove All Mods
		private RelayCommand _commandRemoveAllMods;
		public RelayCommand CommandRemoveAllMods {
			get { return _commandRemoveAllMods ?? (_commandRemoveAllMods = new RelayCommand(ExecuteRemoveAllMods, CanExecuteHasMods)); }
		}
		private void ExecuteRemoveAllMods(object parameter) {
			foreach (ModListBoxItem curMod in Mods.ToList()) {
				string curProjectFile = Path.Combine(_projectsDir, curMod.ProjectFileName);
				Mods.Remove(curMod);
				try {
					File.Delete(curProjectFile);
				} catch (Exception) { }
			}
		}

        // Exploer Mod
        private RelayCommand _commandExploreMod;
        public RelayCommand CommandExploreMod
        {
            get { return _commandExploreMod ?? (_commandExploreMod = new RelayCommand(ExecuteExploreMod, CanExecuteHasSelectedMod)); }
        }
        private void ExecuteExploreMod(object parameter)
        {
            Process.Start("explorer.exe", _selectedMod.ModPath);
        }

        #endregion

        #region Language Command

        // Add Language
        private RelayCommand _commandAddLanguage;
        public RelayCommand CommandAddLanguage
        {
            get { return _commandAddLanguage ?? (_commandAddLanguage = new RelayCommand(ExecuteAddLanguage, CanExecuteHasSelectedMod)); }
        }
        private void ExecuteAddLanguage(object parameter)
        {
            var window = new AddLanguageWindow();
            var dialogResult = window.ShowDialog(View);
            if (dialogResult == true)
            {
                var modPath = _selectedMod.ModPath;
                var languages = _selectedMod.Languages;
                foreach (var lang in window.Result)
                {
                    int splitIndex = lang.IndexOf('(');
                    var realName = lang.Substring(0, splitIndex - 1);
                    var nativeName = lang.Substring(splitIndex + 2, lang.Length - splitIndex - 4);
                    var langItem = new LanguageListBoxItem();
                    langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", realName);
                    langItem.RealName = realName;
                    langItem.NativeName = nativeName;
                    langItem.IsCustom = false;
                    langItem.CustomPath = langItem.LangPath;
                    langItem.IsChecked = true;
                    languages.Add(langItem);
                }
                string projectFile = Path.Combine(_projectsDir, _selectedMod.ProjectFileName);
                _selectedMod.Save(projectFile);
            }
        }

        // Explore Language
        private RelayCommand _commandExploreLanguage;
        public RelayCommand CommandExploreLanguage
        {
            get { return _commandExploreLanguage ?? (_commandExploreLanguage = new RelayCommand(ExecuteExploreLanguage, CanExecuteHasSelectedLanguage)); }
        }
        private void ExecuteExploreLanguage(object parameter)
        {
            //if (_selectedLanguage.IsCustom == true)
            //    Process.Start("explorer.exe", _selectedLanguage.CustomPath);
            //else
                Process.Start("explorer.exe", _selectedLanguage.LangPath);
        }

        private RelayCommand _commandRemoveLanguage;
        public RelayCommand CommandRemoveLanguage
        {
            get { return _commandRemoveLanguage ?? (_commandRemoveLanguage = new RelayCommand(ExecuteRemoveLanguage, CanExecuteHasSelectedLanguage)); }
        }
        private void ExecuteRemoveLanguage(object paremeter)
        {
            _selectedMod.Languages.Remove(_selectedLanguage);
            string projectFile = Path.Combine(_projectsDir, _selectedMod.ProjectFileName);
            try
            {
                _selectedMod.Save(projectFile);
            }
            catch (Exception)
            {
            }
        }

        #endregion



        // Extract
        private RelayCommand _commandExtract;
        public RelayCommand CommandExtract
        {
            get { return _commandExtract ?? (_commandExtract = new RelayCommand(ExecuteExtract, CanExecuteHasSelectedMod)); }
        }
        private void ExecuteExtract(object parameter)
        {
            string projectFile = Path.Combine(_projectsDir, _selectedMod.ProjectFileName);
            string corePath =
                string.IsNullOrWhiteSpace(UserSettings.All.RimWorldInstallDir) ?
                string.Empty :
                Path.Combine(UserSettings.All.RimWorldInstallDir, "Mods", "Core");
            var selectedMod = _selectedMod;
            SelectedMod = null;

            string arguments = $"\"-p:{projectFile}\" -Core:\"{corePath}\"";

            var window = new ExtractWindow();
            window.SelectedMod = selectedMod;
            var dialogResult = window.ShowDialog(View);

            if (dialogResult == true)
            {
                selectedMod.Save(projectFile);
                if (window.IsCleanMode) arguments += " -Clean";
                Process.Start("Trans.exe", arguments);
            }

            SelectedMod = selectedMod;
        }

        // Editor (TODO)

        // Backstory
        private RelayCommand _commandBackstory;
        public RelayCommand CommandBackstory
        {
            get { return _commandBackstory ?? (_commandBackstory = new RelayCommand(ExecuteBackstory)); }
        }
        private void ExecuteBackstory(object parameter)
        {
            var window = new BackstoryWindow();
            window.Show();
        }

        // Tools
        private RelayCommand _commandTools;
        public RelayCommand CommandTools {
            get { return _commandTools ?? (_commandTools = new RelayCommand(ExecuteTools)); }
        }
        private void ExecuteTools(object parameter) {
            var window = new ToolsWindow();
            var dialogResult = window.ShowDialog(View);
        }

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
            if (!Directory.Exists(_projectsDir))
            {
                Directory.CreateDirectory(_projectsDir);
            }
            foreach (ModListBoxItem modItem in Mods)
            {
                if (string.IsNullOrWhiteSpace(modItem.ProjectFileName))
                {
                    modItem.InitialProjectFileName();
                }
                string path = Path.Combine(_projectsDir, modItem.ProjectFileName);
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
