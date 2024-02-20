using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilClasses.Winforms.Extensions;
namespace UtilClasses.Winforms
{
    public class RoundedButton: Button
    {
        private GraphicsPath _path;
        private int _radius;
        private bool _pressed;

        public RoundedButton()
        {
            Radius = 2;
            MouseDown += (s, e) => _pressed = true;
            MouseUp += (s, e) => _pressed = false;
        }

        public int Radius
        {
            get { return _radius; }
            set
            {
                _radius = value; 
                UpdatePath();
            }
        }

        void  UpdatePath()
        {
            var rect = new RectangleF(0, 0, Width, Height);

            _path = new RectangleF(1, 1, Width - 2, Height - 2).GetPath(Radius);
            Region = new Region( rect.GetPath(Radius));
        }



        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.Image == null)
            {
                e.Graphics.FillRectangle(Brushes.Transparent, 0, 0, Width, Height);
                using (var b = new SolidBrush(_pressed ? ForeColor : BackColor))
                    e.Graphics.FillRegion(b, Region);
                using (var p = new Pen(SystemColors.ControlDark, 5f))
                {
                    e.Graphics.DrawPath(p, _path);
                }
                var sz = e.Graphics.MeasureString(Text, Font);
                using (var b = new SolidBrush(_pressed ? BackColor : ForeColor))
                    e.Graphics.DrawString(Text, Font, b, (Width - sz.Width) / 2, (Height - sz.Height) / 2);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdatePath();
        }
    }
}
