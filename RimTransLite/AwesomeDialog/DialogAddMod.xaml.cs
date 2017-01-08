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
using System.IO;
using RimTransLite.AwesomeControl;


namespace RimTransLite.AwesomeDialog
{
    /// <summary>
    /// DialogAddMod.xaml 的交互逻辑
    /// </summary>
    public partial class DialogAddMod : Window
    {
        public DialogAddMod()
        {
            InitializeComponent();
            
        }

        public ModsListItem SelectedMod { get; private set; }

        private ModsListItem.ModCategory selectedModCategory;

        private void buttonInternalMods_Click(object sender, RoutedEventArgs e)
        {
            modsListBox.ClearMods();
            DirectoryInfo dir = new DirectoryInfo(@"D:\Game\Steam\steamapps\common\RimWorld\Mods");
            if (dir.Exists)
            {
                foreach (DirectoryInfo modDir in dir.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    modsListBox.AddMod(ModsListItem.ModCategory.Internal, modDir.FullName, modDir.Name);
                }
            }
        }

        private void buttonWorkshopMods_Click(object sender, RoutedEventArgs e)
        {
            modsListBox.ClearMods();
            DirectoryInfo dir = new DirectoryInfo(@"D:\Game\Steam\steamapps\workshop\content\294100");
            if (dir.Exists)
            {
                foreach (DirectoryInfo modDir in dir.GetDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    modsListBox.AddMod(ModsListItem.ModCategory.Workshop, modDir.FullName, modDir.Name);
                }
            }
        }

        private void modsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModsListItem mod = (ModsListItem)modsListBox.SelectedItem;
            selectedModCategory = mod.Category;
            textBoxName.Text = mod.ModName;
            textBoxPath.Text = mod.ModPath;
        }

        private void buttonCustomMods_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = @"D:\Game\Steam\steamapps\common\RimWorld\Mods";
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                selectedModCategory = ModsListItem.ModCategory.Custom;
                textBoxPath.Text = dlg.SelectedPath;
                textBoxName.Text = System.IO.Path.GetFileName(textBoxPath.Text);
            }
        }

        private void textBoxPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBoxPath = (TextBox)sender;
            if (textBoxPath.Text == string.Empty || textBoxPath.Text == null)
            {
                buttonConfirm.IsEnabled = false;
            }
            else
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(textBoxPath.Text);
                }
                catch (Exception)
                {
                    buttonConfirm.IsEnabled = false;
                }
                finally
                {
                    buttonConfirm.IsEnabled = true;
                }
            }
        }

        private void buttonConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedMod = new ModsListItem(selectedModCategory, textBoxPath.Text, textBoxName.Text);
            this.DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
