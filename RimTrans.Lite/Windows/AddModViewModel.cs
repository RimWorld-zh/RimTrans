using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using duduluu.MVVM;
using RimTrans.Lite.Controls;
using RimTrans.Lite.Util;

namespace RimTrans.Lite.Windows
{
    public class AddModViewModel : ViewModelBase
    {
        public AddModViewModel()
        {
            var modsInternal = new ObservableCollection<ModListBoxItem>();
            var modsWorkshop = new ObservableCollection<ModListBoxItem>();
            string rimworldInstallDir = UserSettings.All.RimWorldInstallDir;
            string internalModsDir = string.Empty;
            if (!string.IsNullOrWhiteSpace(rimworldInstallDir))
            {
                internalModsDir = Path.Combine(rimworldInstallDir, "Mods");
            }
            if (!string.IsNullOrWhiteSpace(internalModsDir) && Directory.Exists(internalModsDir))
            {
                foreach (var modItem in ModListBoxItem.GetModItems(internalModsDir, ModCategory.Internal))
                {
                    modsInternal.Add(modItem);
                }
            }
            string workshopModsDir = UserSettings.All.WorkshopModsDir;
            if (!string.IsNullOrWhiteSpace(workshopModsDir) && Directory.Exists(workshopModsDir))
            {
                foreach (var modItem in ModListBoxItem.GetModItems(workshopModsDir, ModCategory.Workshop))
                {
                    modsWorkshop.Add(modItem);
                }
            }
            ModsInternal = modsInternal;
            ModsWorkshop = modsWorkshop;
        }

        public AddModWindow View { get; set; }

        public ObservableCollection<ModListBoxItem> ModsInternal { get; set; }
        public ObservableCollection<ModListBoxItem> ModsWorkshop { get; set; }

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

        #region Command

        private bool CanExecuteHasSelectedMod(object parameter)
        {
            return _selectedMod != null;
        }

        private RelayCommand _commmandConfirm;
        public RelayCommand CommandConfirm
        {
            get { return _commmandConfirm ?? (_commmandConfirm = new RelayCommand(ExecuteConfirm, CanExecuteHasSelectedMod)); }
        }

        private void ExecuteConfirm(object parameter)
        {
            var all = UserSettings.All;
            var languages = _selectedMod.Languages;
            #region Add Languages
            if (all.Selected_ChineseSimplified)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "ChineseSimplified");
                langItem.RealName = "ChineseSimplified";
                langItem.NativeName = "简体中文";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_ChineseTraditional)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "ChineseTraditional");
                langItem.RealName = "ChineseTraditional";
                langItem.NativeName = "繁體中文";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Czech)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Czech");
                langItem.RealName = "Czech";
                langItem.NativeName = "Čeština";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Danish)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Danish");
                langItem.RealName = "Danish";
                langItem.NativeName = "Dansk";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Dutch)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Dutch");
                langItem.RealName = "Dutch";
                langItem.NativeName = "Nederlands";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_English)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "English");
                langItem.RealName = "English";
                langItem.NativeName = "English";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Estonian)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Estonian");
                langItem.RealName = "Estonian";
                langItem.NativeName = "Eesti";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Finnish)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Finnish");
                langItem.RealName = "Finnish";
                langItem.NativeName = "Suomi";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_French)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "French");
                langItem.RealName = "French";
                langItem.NativeName = "Français";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_German)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "German");
                langItem.RealName = "German";
                langItem.NativeName = "Deutsch";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Hungarian)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Hungarian");
                langItem.RealName = "Hungarian";
                langItem.NativeName = "Magyar";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Italian)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Italian");
                langItem.RealName = "Italian";
                langItem.NativeName = "Italiano";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Japanese)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Japanese");
                langItem.RealName = "Japanese";
                langItem.NativeName = "日本語";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Korean)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Korean");
                langItem.RealName = "Korean";
                langItem.NativeName = "한국어";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Norwegian)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Norwegian");
                langItem.RealName = "Norwegian";
                langItem.NativeName = "Norsk Bokmål";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Polish)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Polish");
                langItem.RealName = "Polish";
                langItem.NativeName = "Polski";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Portuguese)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Portuguese");
                langItem.RealName = "Portuguese";
                langItem.NativeName = "Português";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_PortugueseBrazilian)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "PortugueseBrazilian");
                langItem.RealName = "PortugueseBrazilian";
                langItem.NativeName = "Português Brasileiro";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Romanian)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Romanian");
                langItem.RealName = "Romanian";
                langItem.NativeName = "Română";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Russian)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Russian");
                langItem.RealName = "Russian";
                langItem.NativeName = "Русский";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Slovak)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Slovak");
                langItem.RealName = "Slovak";
                langItem.NativeName = "Slovenčina";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Spanish)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Spanish");
                langItem.RealName = "Spanish";
                langItem.NativeName = "Español(Castellano)";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_SpanishLatin)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "SpanishLatin");
                langItem.RealName = "SpanishLatin";
                langItem.NativeName = "Español(Latinoamérica)";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Swedish)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Swedish");
                langItem.RealName = "Swedish";
                langItem.NativeName = "Svenska";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Turkish)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Turkish");
                langItem.RealName = "Turkish";
                langItem.NativeName = "Türkçe";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            if (all.Selected_Ukrainian)
            {
                var langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(_selectedMod.ModPath, "Languages", "Ukrainian");
                langItem.RealName = "Ukrainian";
                langItem.NativeName = "Українська";
                langItem.IsCustom = false;
                langItem.CustomPath = langItem.LangPath;
                langItem.IsChecked = true;
                languages.Add(langItem);
            }
            #endregion
            View.Result = _selectedMod;
            View.DialogResult = true;
        }

        private RelayCommand _commandSelectAll;
        public RelayCommand CommandSelectAll
        {
            get { return _commandSelectAll ?? (_commandSelectAll = new RelayCommand(ExecuteSelectAll)); }
        }
        private void ExecuteSelectAll(object parameter)
        {
            var all = UserSettings.All;
            #region Select All Languages
            if (all.Selected_ChineseSimplified &&
                all.Selected_ChineseTraditional &&
                all.Selected_Czech &&
                all.Selected_Danish &&
                all.Selected_Dutch &&
                all.Selected_English &&
                all.Selected_Estonian &&
                all.Selected_Finnish &&
                all.Selected_French &&
                all.Selected_German &&
                all.Selected_Hungarian &&
                all.Selected_Italian &&
                all.Selected_Japanese &&
                all.Selected_Korean &&
                all.Selected_Norwegian &&
                all.Selected_Polish &&
                all.Selected_Portuguese &&
                all.Selected_PortugueseBrazilian &&
                all.Selected_Romanian &&
                all.Selected_Russian &&
                all.Selected_Slovak &&
                all.Selected_Spanish &&
                all.Selected_SpanishLatin &&
                all.Selected_Swedish &&
                all.Selected_Turkish &&
                all.Selected_Ukrainian)
            {
                all.Selected_ChineseSimplified = false;
                all.Selected_ChineseTraditional = false;
                all.Selected_Czech = false;
                all.Selected_Danish = false;
                all.Selected_Dutch = false;
                all.Selected_English = false;
                all.Selected_Estonian = false;
                all.Selected_Finnish = false;
                all.Selected_French = false;
                all.Selected_German = false;
                all.Selected_Hungarian = false;
                all.Selected_Italian = false;
                all.Selected_Japanese = false;
                all.Selected_Korean = false;
                all.Selected_Norwegian = false;
                all.Selected_Polish = false;
                all.Selected_Portuguese = false;
                all.Selected_PortugueseBrazilian = false;
                all.Selected_Romanian = false;
                all.Selected_Russian = false;
                all.Selected_Slovak = false;
                all.Selected_Spanish = false;
                all.Selected_SpanishLatin = false;
                all.Selected_Swedish = false;
                all.Selected_Turkish = false;
                all.Selected_Ukrainian = false;
            }
            else
            {
                all.Selected_ChineseSimplified = true;
                all.Selected_ChineseTraditional = true;
                all.Selected_Czech = true;
                all.Selected_Danish = true;
                all.Selected_Dutch = true;
                all.Selected_English = true;
                all.Selected_Estonian = true;
                all.Selected_Finnish = true;
                all.Selected_French = true;
                all.Selected_German = true;
                all.Selected_Hungarian = true;
                all.Selected_Italian = true;
                all.Selected_Japanese = true;
                all.Selected_Korean = true;
                all.Selected_Norwegian = true;
                all.Selected_Polish = true;
                all.Selected_Portuguese = true;
                all.Selected_PortugueseBrazilian = true;
                all.Selected_Romanian = true;
                all.Selected_Russian = true;
                all.Selected_Slovak = true;
                all.Selected_Spanish = true;
                all.Selected_SpanishLatin = true;
                all.Selected_Swedish = true;
                all.Selected_Turkish = true;
                all.Selected_Ukrainian = true;
            }
            #endregion
        }

        #endregion

    }
}
