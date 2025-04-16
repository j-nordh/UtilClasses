using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilClasses.Winforms
{
    public partial class TagList : UserControl
    {
        public TagList()
        {
            InitializeComponent();
        }

        private void lst_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lst.SelectedIndex < 0) return;
            ctxt.Text = lst.SelectedItem.ToString();
        }

        private void ctxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            if (lst.SelectedIndex < 0)
                lst.Items.Add(ctxt.Text);
            else
                lst.Items[lst.SelectedIndex] = ctxt.Text;
            lst.SelectedIndex = -1;
            ctxt.Text = "";
        }

        public string Cue { get { return ctxt.Cue; } set { ctxt.Cue = value; } }
        public IEnumerable<string> Tags => lst.Items.Cast<object>().Select(o => o.ToString());

        public void SetTags(IEnumerable<string> tags)
        {
            lst.Items.Clear();
            if (null == tags) return;
            lst.Items.AddRange(tags.Cast<object>().ToArray());
        }

        private void lst_KeyDown(object sender, KeyEventArgs e)
        {
            if (lst.SelectedIndex < 0) return;
            if (e.KeyCode != Keys.Delete) return;
            lst.Items.RemoveAt(lst.SelectedIndex);
        }
    }
}
