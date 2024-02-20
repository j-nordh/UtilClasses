using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Geometry
{
    public partial class Parallelogram
    {
        public List<Point> Points { get; }
        public int Width { get; }
        public int Height { get; }
        public int X { get; }
        public int Y { get; }
        public double Slant { get; }
        
        private Parallelogram(IEnumerable<Point> ps)
        {
            Points = new List<Point>(ps);
        }

        public Parallelogram(int x, int y, int width, int height, double slant)
        {
            var offset = (int)(Math.Tan(slant * Math.PI / 180) * height);
            Points = new List<Point>() { { x , y }, { x + width, y }, { x - offset + width, y + height }, { x-offset, y + height } };
            Width = width;
            Height = height;
            X = x;
            Y = y;
            Slant = slant;
        }

        public bool Inside(Point p) => Inside(p.X, p.Y);

        public bool Inside(int x, int y)
        {
            var collides = false;
            for (int i = 0; i < Points.Count; i++)
            {
                var c = Points[i];
                var n = Points[(i + 1) % Points.Count];
                if ((c.Y > y) == (n.Y > y)) continue;
                if (x > (n.X - c.X) * (y - c.Y) / (n.Y - c.Y) + c.X) continue;
                collides = !collides;
            }
            return collides;
        }

        public Parallelogram GetFraction(Placement placement, SizeD fraction, Size padding = null) => GetFraction(placement, fraction.Width, fraction.Height, padding);
        public Parallelogram GetFraction(Placement placement, double widthFraction, double heightFraction, Size padding = null)
        {
            var fracWidth = (int)(Width * widthFraction);
            var fracHeight = (int)(Height * heightFraction);
            int x = 0;
            int y = 0;
            if (null == padding) padding = new Size(0, 0);

            var offset = (Points[3].X - Points[0].X) / (Height*1.0);
            //y
            switch (placement)
            {
                case Placement.TopLeft:
                case Placement.TopCenter:
                case Placement.TopRight:
                    y = Y + padding.Height;
                    break;
                case Placement.MiddleLeft:
                case Placement.MiddleCenter:
                case Placement.MiddleRight:
                    y = Y + (Height - fracHeight) / 2;
                    break;
                case Placement.BottomLeft:
                case Placement.BottomCenter:
                case Placement.BottomRight:
                    y = Y + Height - fracHeight - padding.Height;
                    break;
            }

            switch (placement)
            {
                case Placement.TopLeft:
                case Placement.MiddleLeft:
                case Placement.BottomLeft:
                    x = X+padding.Width;
                    break;
                case Placement.TopCenter:
                case Placement.MiddleCenter:
                case Placement.BottomCenter:
                    x = X + (Width - fracWidth) / 2;
                    break;
                case Placement.TopRight:
                case Placement.MiddleRight:
                case Placement.BottomRight:
                    x = X + Width - fracWidth - padding.Width;
                    break;
            }

            x = x+ (int)(offset * (y - Y));

            return new Parallelogram(x, y, fracWidth, fracHeight, Slant);
        }
        public Rectangle Bounds
        {
            get
            {
                var x = Points.Select(p => p.X).Min();
                var y = Points.Select(p => p.Y).Min();
                var width = Points.Select(p => p.X).Max() - x;
                var height = Points.Select(p => p.Y).Max() - y;
                return new Rectangle(x, y, width, height);
            }
        }
        public int Top => Y;
        public int Bottom => Y + Height;
    }
    
}
