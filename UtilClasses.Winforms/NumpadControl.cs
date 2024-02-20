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
    public partial class NumpadControl : UserControl
    {
        public event Action<int> ButtonClick;
        private readonly List<RoundedButton> _buttons;
        private Color _foreColor;
        private Color _backColor;
        public NumpadControl()
        {
            _buttons = new List<RoundedButton>();
            InitializeComponent();
            _foreColor = SystemColors.ControlText;
            _backColor = SystemColors.ButtonFace;
            var index = 0;
            for (int i = 0; i <= 9; i++)
            {
                var btn = new RoundedButton { Tag = i.ToString(), Text = i.ToString(), Dock = DockStyle.Fill };
                btn.Click += ButtonClicked;
                _buttons.Add(btn);

                if (i == 0)
                    tbl.Controls.Add(btn, 1, 3);
                else
                {
                    tbl.Controls.Add(btn, index % 3, index / 3);
                    index += 1;
                }
            }
        }

        public int Radius
        {
            get { return _buttons.First().Radius; }
            set { _buttons.ForEach(b => b.Radius = value); }
        }

        public Font ButtonFont
        {
            get { return _buttons.First().Font; }
            set { _buttons.ForEach(b => b.Font = value); }
        }

        public Color ButtonForeColor
        {
            get { return _foreColor; }
            set
            {
                _buttons.ForEach(b => b.ForeColor = value);
                _foreColor = value;
            }
        }

        public Color ButtonBackColor
        {
            get { return _backColor; }
            set
            {
                _buttons.ForEach(b => b.BackColor = value);
                _backColor = value;
            }
        }

        public void SetButtonImage(int index, Image image)
        {
            if (index < 0 || index > 9)
                throw new ArgumentOutOfRangeException("Index");

            _buttons[index].Image = image;
            _buttons[index].ImageAlign = ContentAlignment.MiddleCenter;
            _buttons[index].Text = "";
            _buttons[index].BackColor = Color.Transparent;
            _buttons[index].FlatStyle = FlatStyle.Flat;
            _buttons[index].FlatAppearance.BorderSize = 0;
        }

        private void ButtonClicked(object sender, EventArgs eventArgs)
        {
            ButtonClick?.Invoke(int.Parse(((RoundedButton) sender).Tag.ToString()));
        }
    }
}
