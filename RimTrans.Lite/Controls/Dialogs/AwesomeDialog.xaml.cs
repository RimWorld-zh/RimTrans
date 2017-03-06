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
using FontAwesome.WPF;

namespace RimTrans.Lite.Controls.Dialogs
{
    /// <summary>
    /// AwesomeDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AwesomeDialog : Window
    {
        public AwesomeDialog()
        {
            InitializeComponent();
        }

        public ButtonTag Result { get; set; } = ButtonTag.Close;

        public bool DoNotPromptResult { get; set; } = false;

        public FontAwesomeIcon AwesomeIcon
        {
            get { return (FontAwesomeIcon)GetValue(AwesomeIconProperty); }
            set { SetValue(AwesomeIconProperty, value); }
        }
        public static readonly DependencyProperty AwesomeIconProperty =
            DependencyProperty.Register("AwesomeIcon", typeof(FontAwesomeIcon), typeof(AwesomeDialog), new PropertyMetadata(FontAwesomeIcon.InfoCircle));



        public ButtonTag ButtonTags
        {
            get { return (ButtonTag)GetValue(ButtonTagsProperty); }
            set { SetValue(ButtonTagsProperty, value); }
        }
        public static readonly DependencyProperty ButtonTagsProperty =
            DependencyProperty.Register("ButtonTags", typeof(ButtonTag), typeof(AwesomeDialog), new PropertyMetadata(ButtonTag.Close));



        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(AwesomeDialog), new PropertyMetadata(string.Empty));



        public bool ShowDoNotPromptCheckBox
        {
            get { return (bool)GetValue(ShowDoNotPromptCheckBoxProperty); }
            set { SetValue(ShowDoNotPromptCheckBoxProperty, value); }
        }
        public static readonly DependencyProperty ShowDoNotPromptCheckBoxProperty =
            DependencyProperty.Register("ShowDoNotPromptCheckBox", typeof(bool), typeof(AwesomeDialog), new PropertyMetadata(false));







        private void buttonDoNotSave_Click(object sender, RoutedEventArgs e)
        {
            Result = ButtonTag.DoNotSave;
            DialogResult = false;
        }

        private void buttonConfirm_Click(object sender, RoutedEventArgs e)
        {
            Result = ButtonTag.Confirm;
            DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = ButtonTag.Cancel;
            DialogResult = false;
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {

            Result = ButtonTag.Close;
            DialogResult = false;
        }

        private void checkBoxNoPrompt_Checked(object sender, RoutedEventArgs e)
        {
            DoNotPromptResult = true;
        }
    }

    public enum ButtonTag
    {
        Close = 0x1,
        Confirm = 0x2,
        Cancel = 0x4,
        DoNotSave = 0x8,
    }
}
