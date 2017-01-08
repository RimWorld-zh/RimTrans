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
using System.Xml;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using FontAwesome.WPF;

namespace RimTransLite.AwesomeControl
{
    #region ModsListItem Class

    public class ModsListItem : ListItem
    {
        public enum ModCategory { Internal, Workshop, Custom }

        public ModsListItem(ModCategory category, string path, string name = null)
        {
            this.Category = category;
            this.ModPath = path;
            if (name == null || name == string.Empty)
            {
                this.ModName = System.IO.Path.GetFileName(path);
            }
            else
            {
                this.ModName = name;
            }
            try
            {
                XDocument aboutDoc = XDocument.Load(System.IO.Path.Combine(path, "About", "About.xml"));
                this.ModNameNatural = aboutDoc.Root.Element("name").Value;
            }
            catch (Exception)
            {
                this.ModNameNatural = System.IO.Path.GetFileName(path);
            }
        }


        public ModCategory Category
        {
            get { return (ModCategory)GetValue(CategoryProperty); }
            set {
                SetValue(CategoryProperty, value);
                switch (value)
                {
                    case ModCategory.Internal:
                        this.Icon = FontAwesomeIcon.FolderOutline;
                        break;
                    case ModCategory.Workshop:
                        this.Icon = FontAwesomeIcon.Steam;
                        break;
                    case ModCategory.Custom:
                        this.Icon = FontAwesomeIcon.HddOutline;
                        break;
                    default:
                        break;
                }
            }
        }

        // Using a DependencyProperty as the backing store for Category.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(ModCategory), typeof(ModsListItem), new PropertyMetadata(ModCategory.Internal));



        public FontAwesomeIcon Icon
        {
            get { return (FontAwesomeIcon)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(FontAwesomeIcon), typeof(ModsListItem), new PropertyMetadata(FontAwesomeIcon.Folder));


        public string ModPath
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Path.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("ModPath", typeof(string), typeof(ModsListItem));


        public string ModName
        {
            get { return (string)GetValue(ModNameProperty); }
            set { SetValue(ModNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModNameProperty =
            DependencyProperty.Register("ModName", typeof(string), typeof(ModsListItem), new PropertyMetadata("Mod"));


        public string ModNameNatural
        {
            get { return (string)GetValue(ModNameNaturalProperty); }
            set { SetValue(ModNameNaturalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NameNatural.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModNameNaturalProperty =
            DependencyProperty.Register("ModNameNatural", typeof(string), typeof(ModsListItem), new PropertyMetadata("Mod"));

        
    }

    #endregion

    /// <summary>
    /// LiteModsListBox.xaml 的交互逻辑
    /// </summary>
    public partial class ModsListBox : ListBox
    {
        public ModsListBox()
        {
            InitializeComponent();
            mods = new ObservableCollection<ModsListItem>();
            this.ItemsSource = mods;
            this.DataContext = this;
        }

        ObservableCollection<ModsListItem> mods;

        public void AddMod(ModsListItem mod)
        {
            mods.Add(mod);
        }

        public void AddMod(ModsListItem.ModCategory category, string path, string name = null)
        {
            mods.Add(new ModsListItem(category, path, name));
        }
        
        public void ClearMods()
        {
            mods.Clear();
        }

        public void RemoveMod(ModsListItem mod)
        {
            mods.Remove(mod);
        }
    }
}
