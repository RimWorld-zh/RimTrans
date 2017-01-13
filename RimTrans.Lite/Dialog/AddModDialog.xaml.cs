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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO;

using duduluu.MVVM;
using RimTransLite.AwesomeControl;


namespace RimTrans.Lite.Dialog
{
    /// <summary>
    /// DialogAddMod.xaml 的交互逻辑
    /// </summary>
    public partial class AddModDialog : Window
    {
        public AddModDialog()
        {
            InitializeComponent();
            var vm = new AddModViewModel();
            vm.View = this;
            this.DataContext = vm;
        }

        public ModsListItem ResultMod { get; set; }


        //private ObservableCollection<ModsListItem> Mods { get; set; }

        //private ObservableCollection<LanguagesListItem> Languages { get; set; }

        //public ModsListItem SelectedMod { get; private set; }

        //private ModsListItem.Category selectedModCategory;

        //private void buttonInternalMods_Click(object sender, RoutedEventArgs e)
        //{
        //    Mods.Clear();
        //    DirectoryInfo dir = new DirectoryInfo(@"D:\Game\Steam\steamapps\common\RimWorld\Mods");
        //    if (dir.Exists)
        //    {
        //        foreach (DirectoryInfo modDir in dir.GetDirectories("*", SearchOption.TopDirectoryOnly))
        //        {
        //            Mods.Add(new ModsListItem(ModsListItem.Category.Internal, modDir.FullName, modDir.Name));
        //        }
        //    }
        //}

        //private void buttonWorkshopMods_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        Mods.Clear();
        //        DirectoryInfo dir = new DirectoryInfo(@"D:\Game\Steam\steamapps\workshop\content\294100");
        //        if (dir.Exists)
        //        {
        //            foreach (DirectoryInfo modDir in dir.GetDirectories("*", SearchOption.TopDirectoryOnly))
        //            {
        //                Mods.Add(new ModsListItem(ModsListItem.Category.Workshop, modDir.FullName, modDir.Name));
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        Mods.Clear();
        //    }

        //}

        //private void buttonCustomMods_Click(object sender, RoutedEventArgs e)
        //{
        //    var dlg = new System.Windows.Forms.FolderBrowserDialog();
        //    dlg.SelectedPath = @"D:\Game\Steam\steamapps\common\RimWorld\Mods";
        //    var result = dlg.ShowDialog();
        //    if (result == System.Windows.Forms.DialogResult.OK)
        //    {
        //        selectedModCategory = ModsListItem.Category.Custom;
        //        textBoxPath.Text = dlg.SelectedPath;
        //        textBoxName.Text = System.IO.Path.GetFileName(textBoxPath.Text);
        //    }
        //}

        //private void modsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    ModsListItem mod = (ModsListItem)modsListBox.SelectedItem;
        //    if (mod != null)
        //    {
        //        selectedModCategory = mod.ModCategory;
        //        textBoxName.Text = mod.ModName;
        //        textBoxPath.Text = mod.ModPath;
        //    }
        //}

        //private void textBoxPath_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    TextBox textBoxPath = (TextBox)sender;
        //    if (textBoxPath.Text == string.Empty || textBoxPath.Text == null)
        //    {
        //        buttonConfirm.IsEnabled = false;
        //    }
        //    else
        //    {
        //        try
        //        {
        //            DirectoryInfo dir = new DirectoryInfo(textBoxPath.Text);
        //        }
        //        catch (Exception)
        //        {
        //            buttonConfirm.IsEnabled = false;
        //        }
        //        finally
        //        {
        //            buttonConfirm.IsEnabled = true;
        //        }
        //    }
        //}

        //private void buttonConfirm_Click(object sender, RoutedEventArgs e)
        //{
        //    this.SelectedMod = new ModsListItem(selectedModCategory, textBoxPath.Text, textBoxName.Text);
        //    foreach (LanguagesListItem lang in LanguagesListItem.PrimaryLanguages())
        //    {
        //        this.SelectedMod.Languages.Add(lang);
        //    }
        //    this.DialogResult = true;
        //}

        //private void buttonCancel_Click(object sender, RoutedEventArgs e)
        //{
        //    this.DialogResult = false;
        //}
    }
}
