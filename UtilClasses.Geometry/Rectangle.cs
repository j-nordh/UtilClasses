using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Geometry
{
    public class Rectangle
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        public int Left => X;
        public int Right => X + Width;
        public int Top => Y;
        public int Bottom => Y + Height;
        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public Rectangle(Point p, int width, int height) : this(p.X, p.Y, width, height) { }
        public Rectangle(Point p, Size s) : this(p.X, p.Y, s.Width, s.Height) { }

        public virtual bool Inside(Point p) => Inside(p.X, p.Y);
        public virtual bool Inside(int x, int y) => x >= Left && x <= Right && y >= Top && y <= Bottom;
        public virtual Size Size => new Size(Width, Height);
        
        public Point TL => new Point(X,Y);
        public Point TR => new Point(Right, Top);
        public Point BL => new Point(Left, Bottom);
        public Point BR => new Point(Right, Bottom);
        public Point LC => new Point(Left, Top + Height / 2);
        public Point RC => new Point(Right, Top + Height / 2);
        public Point TC => new Point(Left + Width / 2, Top);
        public Point BC => new Point(Left + Width / 2, Bottom);
        public Point[] Nodes => new Point[] { TL, TC, TR, RC, BR, BC, BL, LC };
    }
}
