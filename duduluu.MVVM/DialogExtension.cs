using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace duduluu.MVVM
{
    public static class DialogExtension
    {
        public static bool? ShowDialog(this Window win, Window owner)
        {
            win.Owner = owner;
            win.ShowInTaskbar = false;
            return win.ShowDialog();
        }
    }
}
