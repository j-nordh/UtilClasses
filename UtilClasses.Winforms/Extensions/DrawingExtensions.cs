using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Winforms.Extensions
{
    public static class DrawingExtensions
    {
        public static PointF Add(this PointF p1, PointF p2) => new PointF(p1.X + p2.X, p1.Y + p2.Y);
        public static PointF Subtract(this PointF p1, PointF p2) => new PointF(p1.X - p2.X, p1.Y - p2.Y);

        public static PointF Divide(this PointF p, float f) => new PointF(p.X / f, p.Y / f);
        public static SizeF Divide(this SizeF s, float f)=>new SizeF(s.Width/f, s.Height/f);

        public static RectangleF ToRectangleF(this IEnumerable<PointF> points)
        {
            var ps = points as IList<PointF> ?? points.ToList();
            var left = ps.Select(p => p.X).Min();
            var right = ps.Select(p => p.X).Max();
            var top = ps.Select(p => p.Y).Min();
            var bottom = ps.Select(p => p.Y).Max();

            return new RectangleF(left,top,right-left, bottom-top);
        }

        public static Rectangle Truncate(this RectangleF rect) => Rectangle.Truncate(rect);

        public static Point Move(this Point p, Size sz) => new Point(p.X + sz.Width, p.Y + sz.Height);
        public static Point MoveY(this Point p, int dy)=>new Point(p.X, p.Y+dy);
        public static Point MoveX(this Point p, int dx) => new Point(p.X + dx, p.Y);

        public static IEnumerable<Point> Move(this IEnumerable<Point> points, Point offset) => points.Select(p => new Point(offset.X + p.X, offset.Y + p.Y));
        public static Rectangle Grow(this Rectangle r, int dx, int dy) => new Rectangle(r.Left - dx, r.Top - dy, r.Width + 2 * dx, r.Height + 2 * dy);
        public static Rectangle Grow(this Rectangle r, int d) => r.Grow(d, d);
        public static GraphicsPath GetPath(this RectangleF rect, float radius)
        {
            float r2 = radius / 2f;
            var graphPath = new GraphicsPath();
            var t = rect.Y;
            var l = rect.X;
            var b = rect.Y + rect.Height;
            var r = rect.X + rect.Width;

            graphPath.AddArc(l, t, radius, radius, 180, 90); //tl
            graphPath.AddLine(l + r2, t, r - r2, t);
            graphPath.AddArc(r - radius, t, radius, radius, 270, 90); //tr
            graphPath.AddLine(r, t + r2, r, b - r2);
            graphPath.AddArc(r - radius, b - radius, radius, radius, 0, 90); //br
            graphPath.AddLine(r - r2, b, l + r2, b);
            graphPath.AddArc(l, b - radius, radius, radius, 90, 90); //bl
            graphPath.AddLine(l, b - r2, l, t + r2);

            graphPath.CloseFigure();
            return graphPath;
        }
        public static GraphicsPath GetPath(this Rectangle rect, float radius) => ((RectangleF)rect).GetPath(radius);
    }
}
