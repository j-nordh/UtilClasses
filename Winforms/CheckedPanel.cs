using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Winforms.Extensions;

namespace UtilClasses.Winforms
{
    public class CheckedPanel : Panel
    {
        private CheckBox _chk;
        private Label _lbl;

        public event EventHandler CheckedChanged; 
        public CheckedPanel()
        {
            //5; 15; 5; 5
            Padding=new Padding(5,15,5,5);
            _chk = new CheckBox
            {
                Location = new Point(15, 0),
                Size = new Size(15, 15),
                AutoSize = false
            };
            _chk.CheckedChanged +=OnCheckedChanged;
            _lbl = new Label
            {
                Location = new Point(30, 0),
                AutoSize = true
            };
            _lbl.AutoSizeChanged += (sender, args) => Invalidate();
            _lbl.Click += (s, e) =>
            {
                if (_chk.Visible && _chk.Enabled)
                    _chk.Checked = !_chk.Checked;
            };
            Controls.Add(_chk);
            Controls.Add(_lbl);
            Layout += (sender, args) => OnCheckedChanged();
        }

        

        private void OnCheckedChanged() => OnCheckedChanged(null, null);
        private void OnCheckedChanged(object sender, EventArgs e)
        {
            CheckedChanged?.Invoke(sender,e);
            Controls.Cast<Control>().Where(c=>c!=_chk && c!=_lbl).ForEach(c=>c.Enabled=_chk.Checked);
        }

        public bool Checked
        {
            get { return _chk.Checked; }
            set { _chk.Checked = value; }
        }

        [Bindable(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override string Text
        {
            get { return _lbl.Text; }
            set { _lbl.Text = value; }
        }

        public bool HideCheckbox { get => !_chk.Visible; set => _chk.Visible = !value; }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(BackColor),DisplayRectangle);
            var t = _chk.Height / 2;
            var l = 2;
            var r = ClientRectangle.Right - 2;
            var b = ClientRectangle.Bottom - 2;
            var radius = 4;
            var path = new GraphicsPath();
            int diameter = radius * 2;
            var arc = new RectangleF(r - diameter, t , diameter, diameter);

            path.AddLine(_lbl.Right, t, r - diameter, t);
            // top right arc  
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = b - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = l;
            path.AddArc(arc, 90, 90);

            // top left arc 
            arc.Y = t; 
            path.AddArc(arc, 180, 90);
            path.AddLine(l+diameter, t, _chk.Left-2,t);
            e.Graphics.DrawPath(Pens.CornflowerBlue,path);

            //var ps = new List<Point>()
            //{
            //    {_chk.Right, t},
            //    {r , t},
            //    {r, b},
            //    {l, b},
            //    {l, t},
            //    {_chk.Left, t}
            //};
            //e.Graphics.DrawLines(Pens.CornflowerBlue, ps.ToArray());
        }
    }

}
