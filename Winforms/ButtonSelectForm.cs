using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UtilClasses.Core.Extensions.Enums;

namespace UtilClasses.Winforms
{
    public partial class ButtonSelectForm <T>: Form where T:struct
    {
        public ButtonSelectForm()
        {
            InitializeComponent();
            Load += ButtonSelectForm_Load;
        }

        private void ButtonSelectForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private int _x;
        public T? SelectedValue { get; private set; }

        public void Init(Func<T, string> namer = null)
        {
            panel1.Controls.Clear();
            var pad = new Padding(8);
            _x = 0;
            foreach (var opt in EnumExtensions.Values<T>())
            {
                AddButton(namer?.Invoke(opt) ?? opt.ToString(), pad, opt);
            }
            var lastBtn = AddButton("Cancel", pad, null);
            var offset = (panel1.ClientRectangle.Right - lastBtn.Right) / 2;
            if (offset > 0)
            {
                foreach (var btn in panel1.Controls.OfType<Button>())
                {
                    btn.Left += offset;
                }
            }

        }

        private Button AddButton(string caption, Padding pad, T? value)
        {
            var btn = new Button
            {
                Location = new Point(_x, pad.Top),
                Text = caption,
                MinimumSize = new Size(50, 23),
                AutoSize = true,
                Tag = value
            };

            btn.Click += (s, e) =>
            {
                SelectedValue = value;
                DialogResult = value == null ? DialogResult.Cancel : DialogResult.OK;
                Close();
            };
           
            panel1.Controls.Add(btn);
            _x = btn.Right + pad.Horizontal;
            return btn;
        }


        public string Question
        {
            get { return lbl.Text; }
            set { lbl.Text = value; }
        }
    }
}
