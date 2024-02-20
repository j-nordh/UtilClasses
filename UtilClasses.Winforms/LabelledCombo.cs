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
    public partial class LabelledCombo : UserControl
    {
        public event Action SelectedIndexChanged;
        public LabelledCombo()
        {
            InitializeComponent();
        }
        public string Caption
        {
            get => ulbl.Text;
            set
            {
                ulbl.Text = value;
                ulbl.Width = TextRenderer.MeasureText(value, ulbl.Font).Width;
            }
        }
        public ContentAlignment CaptionAlignment
        {
            get => ulbl.TextAlign;
            set => ulbl.TextAlign = value;
        }
        public ComboBoxStyle DropDownStyle
        {
            get => cmbo.DropDownStyle;
            set => cmbo.DropDownStyle = value;
        }

        public void AddRange(IEnumerable<string> items)
        {
            var lst = items as IList<string> ?? items.ToList();
            cmbo.Items.AddRange(lst.Cast<object>().ToArray());
            if (cmbo.Items.Cast<string>().Any())
                cmbo.DropDownWidth = lst.Max(s => TextRenderer.MeasureText(s, cmbo.Font).Width);
        }

        public void AddRange(IEnumerable<object> items)
        {
            var lst = items as IList<object> ?? items.ToList();
            cmbo.Items.AddRange(lst.ToArray());
            if (lst.Any())
                cmbo.DropDownWidth = lst.Max(s => TextRenderer.MeasureText(s.ToString(), cmbo.Font).Width);
        }


        public string DisplayMember { get => cmbo.DisplayMember; set => cmbo.DisplayMember = value; }

        public void ClearItems() => cmbo.Items.Clear();
        public void SetItems(IEnumerable<string> items)
        {
            ClearItems();
            AddRange(items);
        }
        public void SetItems(IEnumerable<object> items)
        {
            ClearItems();
            AddRange(items);
        }
        public string SelectedString => cmbo.SelectedItem?.ToString();
        public object SelectedObject { get => cmbo.SelectedItem; set => cmbo.SelectedItem = value; }
        public int SelectedIndex
        {
            get => cmbo.SelectedIndex;
            set => cmbo.SelectedIndex = value;
        }
        public T GetSelected<T>() => (T)cmbo.SelectedItem;

        public DockStyle CaptionDockStyle
        {
            get { return ulbl.Dock; }
            set
            {
                ulbl.Dock = value;
                if (CaptionDockStyle == DockStyle.Left || CaptionDockStyle == DockStyle.Right)
                    ulbl.TextAlign = ContentAlignment.MiddleCenter;
                else
                    ulbl.TextAlign = ContentAlignment.TopLeft;
            }
        }

        public void Select(string s)
        {
            cmbo.SelectedIndex = cmbo.FindStringExact(s);
        }

        private void cmbo_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedIndexChanged?.Invoke();
        }
    }
}
