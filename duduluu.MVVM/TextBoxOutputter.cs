using System;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace duduluu.MVVM
{
    public class TextBoxOutputter : TextWriter
    {
        public TextBoxOutputter(TextBox output)
        {
            textBox = output;
        }

        TextBox textBox;

        public override void Write(char value)
        {
            base.Write(value);
            textBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.AppendText(value.ToString());
            }));
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
