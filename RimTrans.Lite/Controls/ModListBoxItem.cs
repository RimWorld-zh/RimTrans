using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using System.Xml;
using System.Xml.Linq;
using FontAwesome.WPF;
using RimTrans.Lite.Util;

namespace RimTrans.Lite.Controls
{
    /// <summary>
    /// ListBoxItem for showing mods.
    /// </summary>
    public class ModListBoxItem : ListBoxItem
    {
        public ModListBoxItem()
        {
            Languages = new ObservableCollection<LanguageListBoxItem>();
        }

        public ObservableCollection<LanguageListBoxItem> Languages { get; set; }

        #region Porperties

        /// <summary>
        /// The directory path of the mod.
        /// </summary>
        [Bindable(true), Category("Common"), Description("The text of the button.")]
        public string ModPath
        {
            get { return (string)GetValue(ModPathProperty); }
            set { SetValue(ModPathProperty, value); }
        }
        public static readonly DependencyProperty ModPathProperty =
            DependencyProperty.Register("ModPath", typeof(string), typeof(ModListBoxItem));


        /// <summary>
        /// Where the mod is.
        /// </summary>
        [Bindable(true), Category("Common"), Description("The text of the button.")]
        public ModCategory Category
        {
            get { return (ModCategory)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }
        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(ModCategory), typeof(ModListBoxItem));


        /// <summary>
        /// The mod's display name.
        /// </summary>
        [Bindable(true), Category("Common"), Description("The text of the button.")]
        public string ModName
        {
            get { return (string)GetValue(ModNameProperty); }
            set { SetValue(ModNameProperty, value); }
        }
        public static readonly DependencyProperty ModNameProperty =
            DependencyProperty.Register("ModName", typeof(string), typeof(ModListBoxItem));


        /// <summary>
        /// Generate Mode
        /// </summary>
        [Bindable(true), Category("Common"), Description("The text of the button.")]
        public GenerateMode GenerateOption
        {
            get { return (GenerateMode)GetValue(GenerateOptionProperty); }
            set { SetValue(GenerateOptionProperty, value); }
        }
        public static readonly DependencyProperty GenerateOptionProperty =
            DependencyProperty.Register("GenerateOption", typeof(GenerateMode), typeof(ModListBoxItem));


        ///// <summary>
        ///// Delete the old files or not when generate.
        ///// </summary>
        //[Bindable(true), Category("Common"), Description("The text of the button.")]
        //public bool IsCleanMode
        //{
        //    get { return (bool)GetValue(IsCleanModeProperty); }
        //    set { SetValue(IsCleanModeProperty, value); }
        //}
        //public static readonly DependencyProperty IsCleanModeProperty =
        //    DependencyProperty.Register("IsCleanMode", typeof(bool), typeof(ModListBoxItem));

        #endregion

        #region Save

        public void Save()
        {

        }

        #endregion

        #region Load from file

        public static ModListBoxItem Load(string path)
        {
            ModListBoxItem modItem = new ModListBoxItem();

            if (File.Exists(path))
            {
                XDocument doc = XDocument.Load(path);
                XElement root = doc.Root;
                {
                    string modPath = root.Element("ModPath").Value;
                    if (!string.IsNullOrWhiteSpace(modPath))
                        modItem.ModPath = modPath;
                    else
                        throw new Exception("Invalid value.");

                    ModCategory modCategory;
                    if (Enum.TryParse(root.Element("Category").Value, out modCategory))
                        modItem.Category = modCategory;
                    else
                        throw new Exception("Invalid value.");

                    GenerateMode modGenerateOption;
                    if (Enum.TryParse(root.Element("GenerateOption").Value, out modGenerateOption))
                        modItem.GenerateOption = modGenerateOption;
                    else
                        throw new Exception("Invalid value.");
                }
                XElement languages = root.Element("Languages");
                foreach (XElement curLang in languages.Elements())
                {
                    LanguageListBoxItem langItem = new LanguageListBoxItem();

                    string langPath = curLang.Element("LangPath").Value;
                    if (!string.IsNullOrWhiteSpace(langPath))
                        langItem.LangPath = langPath;
                    else
                        throw new Exception("Invalid value.");

                    string langRealName = curLang.Element("RealName").Value;
                    if (!string.IsNullOrWhiteSpace(langRealName))
                        langItem.RealName = langRealName;
                    else
                        throw new Exception("Invalid value.");

                    string langNativeName = curLang.Element("NativeName").Value;
                    if (!string.IsNullOrWhiteSpace(langNativeName))
                        langItem.NativeName = langNativeName;
                    else
                        throw new Exception("Invalid value.");

                    bool langIsCustom;
                    if (bool.TryParse(root.Element("IsCustom").Value, out langIsCustom))
                        langItem.IsCustom = langIsCustom;
                    else
                        langItem.IsCustom = false;

                    string langCustomPath = root.Element("CustomPath").Value;
                    if (!string.IsNullOrWhiteSpace(langCustomPath))
                        langItem.CustomPath = langCustomPath;
                    else
                        throw new Exception("Invalid value.");

                    bool langIsChecked;
                    if (bool.TryParse(root.Element("IsChecked").Value, out langIsChecked))
                        langItem.IsChecked = langIsChecked;
                    else
                        langItem.IsChecked = false;

                    modItem.Languages.Add(langItem);
                }
            }
            else
            {
                throw new Exception("The File dose not exist: " + path);
            }

            return modItem;
        }

        #endregion

        #region Get mods from directory
        
        public static IEnumerable<ModListBoxItem> GetModItems(string path, ModCategory category)
        {
            foreach (string subDir in Directory.GetDirectories(path))
            {
                ModListBoxItem modItem = new ModListBoxItem();
                
                modItem.ModPath = subDir;
                modItem.Category = category;
                modItem.ModName = GetModName(subDir);
                if (System.IO.Path.GetFileName(subDir) == "Core")
                    modItem.GenerateOption = GenerateMode.Core;
                else
                    modItem.GenerateOption = GenerateMode.Standard;

                yield return modItem;
            }
        }

        public static string GetModName(string path)
        {
            string about = Path.Combine(path, "About", "About.xml");
            if (File.Exists(about))
            {
                try
                {
                    XDocument doc = XDocument.Load(about);
                    XElement name = doc.Root.Element("name");
                    if (name != null)
                    {
                        return name.Value;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion
    }
}
