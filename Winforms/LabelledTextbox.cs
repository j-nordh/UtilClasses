using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UtilClasses.Winforms
{
    public partial class LabelledTextbox
    {
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public new event EventHandler TextChanged;
        public LabelledTextbox()
        {
            InitializeComponent();
            utxt.TextChanged += (s, e) => TextChanged?.Invoke(s, e);
        }


        public string Caption
        {
            get { return ulbl.Text; }
            set
            {
                ulbl.Text = value;
                ulbl.Width = TextRenderer.MeasureText(value, ulbl.Font).Width;
            }
        }
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        public override string Text
        {
            get => utxt.Text;
            set => utxt.Text = value;
        }

        public bool Multiline
        {
            get => utxt.Multiline;
            set => utxt.Multiline = value;
        }

        public bool UsePasswordChar { get => utxt.UseSystemPasswordChar; set => utxt.UseSystemPasswordChar = value; }
        
        public bool ReadOnly
        {
            get => utxt.ReadOnly;
            set => utxt.ReadOnly = value;
        }

        public string UnitText
        {
            get => ulUnit.Text;
            set => ulUnit.Text = value;
        }
        public DockStyle CaptionDockStyle
        {
            get => ulbl.Dock;
            set
            {
                ulbl.Dock = value;
                if (value == DockStyle.Left || value == DockStyle.Right)
                    ulbl.TextAlign = ContentAlignment.MiddleCenter;
                else
                    ulbl.TextAlign = ContentAlignment.TopLeft;
            }
        }
    }
}
