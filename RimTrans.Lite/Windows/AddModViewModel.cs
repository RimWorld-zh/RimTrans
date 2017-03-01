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
            ModsInternal = new ObservableCollection<ModListBoxItem>();
            ModsWorkshop = new ObservableCollection<ModListBoxItem>();
            string dirInternal = RimWorldHelper.GetInternalModsDir();
            if (!string.IsNullOrWhiteSpace(dirInternal) && Directory.Exists(dirInternal))
            {
                foreach (var modItem in ModListBoxItem.GetModItems(dirInternal, ModCategory.Internal))
                {
                    ModsInternal.Add(modItem);
                }
            }
            string dirWorkshop = UserSettings.All.Workshop294100;
            if (!string.IsNullOrWhiteSpace(dirWorkshop) && Directory.Exists(dirWorkshop))
            {
                foreach (var modItem in ModListBoxItem.GetModItems(dirWorkshop, ModCategory.Workshop))
                {
                    ModsWorkshop.Add(modItem);
                }
            }
        }

        public AddModWindow View { get; set; }

        public ObservableCollection<ModListBoxItem> ModsInternal { get; set; }
        public ObservableCollection<ModListBoxItem> ModsWorkshop { get; set; }

    }
}
