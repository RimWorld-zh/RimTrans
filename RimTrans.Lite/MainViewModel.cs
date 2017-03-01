using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using duduluu.MVVM;
using RimTrans.Lite.Controls;

namespace RimTrans.Lite
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Mods = new ObservableCollection<ModListBoxItem>();
            foreach (var modItem in ModListBoxItem.GetModItems(@"D:\Game\Steam\steamapps\common\RimWorld\Mods", Util.ModCategory.Workshop))
            {
                LanguageListBoxItem langItem = new LanguageListBoxItem();
                langItem.LangPath = Path.Combine(modItem.ModPath, "Languages", "ChineseSimplified");
                langItem.RealName = "ChineseSimplified";
                langItem.NativeName = "简体中文";
                langItem.CustomPath = langItem.LangPath;
                modItem.Languages.Add(langItem);
                Mods.Add(modItem);
            }
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




    }
}
