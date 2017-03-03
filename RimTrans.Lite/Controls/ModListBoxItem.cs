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

        #region Hash

        public string ProjectFileName { get; set; }

        public void InitialProjectFileName()
        {
            this.ProjectFileName =
                this.Category.ToString() +
                "_" +
                Path.GetFileName(this.ModPath) +
                string.Format("{0:_yyyyMMdd_HHmmss}", DateTime.Now) +
                ".xml";
        }

        #endregion

        #region Porperties

        /// <summary>
        /// The directory path of the mod.
        /// </summary>
        [Bindable(true), Category("Common"), Description("The path of the mod.")]
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
        [Bindable(true), Category("Common"), Description("The category of the mod.")]
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
        [Bindable(true), Category("Common"), Description("The name of the mod.")]
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
        [Bindable(true), Category("Common"), Description("The generate option of the mod.")]
        public GenerateMode GenerateOption
        {
            get { return (GenerateMode)GetValue(GenerateOptionProperty); }
            set { SetValue(GenerateOptionProperty, value); }
        }
        public static readonly DependencyProperty GenerateOptionProperty =
            DependencyProperty.Register("GenerateOption", typeof(GenerateMode), typeof(ModListBoxItem));

        
        ///// <summary>
        ///// Clean Mode
        ///// </summary>
        //[Bindable(true), Category("Common"), Description("If clean or not when generate.")]
        //public bool? IsCleanMode
        //{
        //    get { return (bool?)GetValue(IsCleanModeProperty); }
        //    set { SetValue(IsCleanModeProperty, value); }
        //}
        //public static readonly DependencyProperty IsCleanModeProperty =
        //    DependencyProperty.Register("IsCleanMode", typeof(bool?), typeof(ModListBoxItem));
        

        #endregion

        #region Save

        public void Save(string path)
        {
            XElement languages = new XElement("Languages");
            foreach (LanguageListBoxItem langItem in this.Languages)
            {
                XElement li = new XElement("li",
                    new XElement("LangPath", langItem.LangPath),
                    new XElement("RealName", langItem.RealName),
                    new XElement("NativeName", langItem.NativeName),
                    new XElement("IsCustom", langItem.IsCustom),
                    new XElement("CustomPath", langItem.CustomPath),
                    new XElement("IsChecked", langItem.IsChecked));
                languages.Add(li);
            }
            XElement root = new XElement("ProjectMod",
                new XElement("ModPath", this.ModPath),
                new XElement("Category", this.Category),
                new XElement("GenerateOption", this.GenerateOption));
            root.Add(languages);
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
            doc.Save(path);
        }

        #endregion

        #region Load from file

        public static ModListBoxItem Load(string path)
        {

            if (File.Exists(path))
            {
                ModListBoxItem modItem = new ModListBoxItem();
                XDocument doc = XDocument.Load(path);
                XElement root = doc.Root;
                modItem.ModPath = root.Element("ModPath").Value;
                XElement category = root.Element("Category");
                switch (category.Value)
                {
                    case "Internal":
                        modItem.Category = ModCategory.Internal;
                        break;
                    case "Workshop":
                        modItem.Category = ModCategory.Workshop;
                        break;
                    default:
                        modItem.Category = ModCategory.Custom;
                        break;
                }
                XElement generateOption = root.Element("GenerateOption");
                switch (generateOption.Value)
                {
                    case "Special":
                        modItem.GenerateOption = GenerateMode.Special;
                        break;
                    case "Core":
                        modItem.GenerateOption = GenerateMode.Core;
                        break;
                    default:
                        modItem.GenerateOption = GenerateMode.Standard;
                        break;
                }
                XElement languages = root.Element("Languages");
                foreach (XElement li in languages.Elements())
                {
                    LanguageListBoxItem langItem = new LanguageListBoxItem();
                    langItem.LangPath = li.Element("LangPath").Value;
                    langItem.RealName = li.Element("RealName").Value;
                    langItem.NativeName = li.Element("NativeName").Value;
                    langItem.IsCustom = (string.Compare(li.Element("IsCustom").Value, "true", true) == 0);
                    langItem.CustomPath = li.Element("CustomPath").Value;
                    langItem.IsChecked = (string.Compare(li.Element("IsChecked").Value, "true", true) == 0);
                    modItem.Languages.Add(langItem);
                }
                modItem.ProjectFileName = Path.GetFileName(path);
                modItem.ModName = GetModName(modItem.ModPath);
                return modItem;
            }
            else
            {
                throw new Exception("The File dose not exist: " + path);
            }
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
                //modItem.IsCleanMode = false;
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
