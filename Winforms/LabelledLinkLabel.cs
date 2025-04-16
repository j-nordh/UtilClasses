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
    public partial class LabelledLinkLabel : UserControl
    {
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event LinkLabelLinkClickedEventHandler LinkClicked;
        public LabelledLinkLabel()
        {
            InitializeComponent();
            link.LinkClicked += (_, e) => LinkClicked?.Invoke(this, e);
        }

        public string Caption
        {
            get => ulbl.Text;
            set
            {
                ulbl.Text = value;
                MinimumSize = new Size(TextRenderer.MeasureText(value, ulbl.Font).Width+2, MinimumSize.Height);
            }
        }
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override string Text { get => link.Text; set => link.Text = value; }
    }
}
