using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Geometry
{
    public class Point
    {
        public int X { get; }
        public int Y { get; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point MoveX(int dx) => new Point(X + dx, Y);
        public Point MoveY(int dy) => new Point(X, Y + dy);

        public static Point Origin=> new Point(0, 0);
        public bool Hits(Point other, int radius) => Math.Abs(X - other.X) <= radius && Math.Abs(Y - other.Y) <= radius;
    }
}
