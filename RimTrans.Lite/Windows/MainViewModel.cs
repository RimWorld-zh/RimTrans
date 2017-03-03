using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using duduluu.MVVM;
using RimTrans.Lite.Controls;

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
