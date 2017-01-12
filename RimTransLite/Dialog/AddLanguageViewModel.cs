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
using System.Collections.ObjectModel;

using FontAwesome.WPF;
using duduluu.MVVM;
using RimTransLite.AwesomeControl;
using RimTransLite.Options;

namespace RimTrans.Lite.Dialog
{
    class AddLanguageViewModel : ViewModelBase
    {
        public AddLanguageViewModel()
        {
            PrimaryLanguages = LanguagesListItem.PrimaryLanguages();
        }

        public AddLanguageDialog View { get; set; }

        public ObservableCollection<LanguagesListItem> PrimaryLanguages { get; set; }

        // Command Add Custom Language
        private RelayCommand _addCustomLanguageCommand;
        public RelayCommand AddCustomLanguageCommand
        {
            get
            {
                return _addCustomLanguageCommand ?? (_addCustomLanguageCommand = new RelayCommand(ExecuteAddCustomLanguae));
            }
        }
        private void ExecuteAddCustomLanguae(object parameter)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                LanguagesListItem customLang = new LanguagesListItem(
                        System.IO.Path.GetFileName(dlg.SelectedPath),
                        null,
                        dlg.SelectedPath);
                customLang.IsChecked = true;
                View.ResultLanguages = new List<LanguagesListItem> { customLang };
                View.DialogResult = true;
            }
        }

        // Command Confirm
        private RelayCommand _confirmCommand;
        public RelayCommand ConfirmCommand
        {
            get
            {
                return _confirmCommand ?? (_confirmCommand = new RelayCommand(ExecuteConfirm));
            }
        }
        private void ExecuteConfirm(object parameter)
        {
            View.ResultLanguages = from lang in PrimaryLanguages
                                   where lang.IsChecked
                                   select lang;
            View.DialogResult = true;
        }
    }
}
