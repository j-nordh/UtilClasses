using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UtilClasses.Winforms
{
    public partial class LabelledLabel : UserControl
    {
        public LabelledLabel()
        {
            InitializeComponent();
        }

        public string Caption { get => lblCaption.Text; set { lblCaption.Text = value; CalcMinSize(); } }
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override string Text { get => lblContent.Text; set { lblContent.Text = value; CalcMinSize(); } }
        private void CalcMinSize()
        {
            var h = lblCaption.Height + Padding.Vertical + CalcHeight(lblContent);
            var w = Math.Max(CalcWidth(lblCaption), CalcWidth(lblContent));
            MinimumSize = new Size(w, h);
        }
        private int CalcWidth(Control c)
        {
            return TextRenderer.MeasureText(c.Text, c.Font).Width + c.Padding.Horizontal + c.Margin.Horizontal + Padding.Horizontal;
        }
        private int CalcHeight(Control c) => TextRenderer.MeasureText(c.Text, c.Font).Height + c.Padding.Vertical + c.Margin.Vertical + Padding.Vertical;
    }
}
