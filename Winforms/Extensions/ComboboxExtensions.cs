using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilClasses.Winforms.Extensions
{
    public static class ComboboxExtensions
    {
        public static bool SelectItem<T>(this ComboBox cb, T key, IComparer<T> cmp) => cb.SelectItem<T>(o => cmp.Compare(o, key) == 0);

        public static bool SelectItem(this ComboBox cb, string key, StringComparer cmp = null) => cb.SelectItem<string>(key, cmp ?? StringComparer.OrdinalIgnoreCase);
        public static bool SelectItem<T>(this ComboBox cb, Func<T, bool> predicate)
        {
            for(int i =0;i<cb.Items.Count;i++)
            {
                if (!predicate((T)cb.Items[i])) continue;
                cb.SelectedIndex = i;
                return true;
            }
            return false;
        }
    }
}
