using System;
using System.Drawing;
using System.Windows.Forms;

namespace UtilClasses.Winforms
{
    public class VerticalLabel:Label
    {
        public VerticalLabel()
        {
            RotateFlip = RotateFlipType.Rotate270FlipNone;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var fs = e.Graphics.MeasureString(Text, Font);
            var s = new Size((int)fs.Width, (int)fs.Height);
            
            var bmp = new Bitmap(s.Width, s.Height);
            var g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(BackColor),0,0,s.Width, s.Height);
            g.DrawString(Text, Font, new SolidBrush(ForeColor), 0, 0);
            g.Flush();
            bmp.RotateFlip(RotateFlip);
            e.Graphics.FillRectangle(new SolidBrush(BackColor),0,0,Width, Height);
            e.Graphics.DrawImage(bmp, GetP(s));
        }
        public RotateFlipType RotateFlip { get; set; }
        Point GetP(Size orgTextSize)
        {
            var centerY = (Height - orgTextSize.Width) / 2;
            var centerX = (Width - orgTextSize.Height) / 2;
            var leftY = Height - orgTextSize.Width;
            var bottomX = Width - orgTextSize.Height;
            switch (TextAlign)
            {
                case ContentAlignment.TopLeft:
                    return new Point(0, leftY);
                case ContentAlignment.TopCenter:
                    return new Point(0, centerY);
                case ContentAlignment.TopRight:
                    return new Point(0, 0);
                case ContentAlignment.MiddleLeft:
                    return new Point(centerX, leftY);
                case ContentAlignment.MiddleCenter:
                    return new Point(centerX, centerY);
                case ContentAlignment.MiddleRight:
                    return new Point(centerX, 0);
                case ContentAlignment.BottomLeft:
                    return new Point(bottomX, leftY);
                case ContentAlignment.BottomCenter:
                    return new Point(bottomX, centerY);
                case ContentAlignment.BottomRight:
                    return new Point(bottomX, 0);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
