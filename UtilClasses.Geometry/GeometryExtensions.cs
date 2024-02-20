using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Geometry
{
    public static class GeometryExtensions
    {
        public static void Add(this List<Point> lst , int x, int y)
        {
            lst.Add(new Point(x, y));
        }
    }
}
