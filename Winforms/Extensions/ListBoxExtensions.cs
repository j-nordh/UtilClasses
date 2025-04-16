using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilClasses.Winforms.Extensions
{
    public static class ListBoxExtensions
    {
        public static void MoveItem(this ListBox lst,  int direction)
        {
            // Checking selected item
            if (lst.SelectedItem == null || lst.SelectedIndex < 0)
                return; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = lst.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= lst.Items.Count)
                return; // Index out of range - nothing to do

            var selected = lst.SelectedItem;

            // Removing removable element
            lst.Items.Remove(selected);
            // Insert it in new position
            lst.Items.Insert(newIndex, selected);
            // Restore selection
            lst.SetSelected(newIndex, true);
        }

        public static void SetItems(this ListBox lst, IEnumerable<object> os)
        {
            lst.Items.Clear();
            lst.Items.AddRange(os as object[] ?? os?.ToArray() ?? new object[] { });
        }
    }
}
