using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilClasses.Core.Extensions.Enums;

namespace UtilClasses.Winforms.Extensions
{
    public static class Enums
    {
        public static void Set<T>(this ComboBox.ObjectCollection oc)
        {
            if (!typeof(T).IsEnum) throw new Exception($"The supplied type {typeof(T).Name} is not an Enum.");
            oc.Clear();
            oc.AddRange(Enum.GetNames(typeof(T)).Cast<object>().ToArray());
        }

        public static T? Selected<T>(this ComboBox cmbo) where T: struct
        {
            if (!typeof(T).IsEnum) throw new Exception($"The supplied type {typeof(T).Name} is not an Enum.");
            return cmbo.SelectedItem?.ToString().ParseAs<T>();
        }
    }
}
