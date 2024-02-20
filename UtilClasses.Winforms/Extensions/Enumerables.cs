using System.Collections.Generic;
using System.Drawing;

namespace UtilClasses.Winforms.Extensions
{
    public static class Enumerables
    {
        public static void Add(this List<Point> lst, int x, int y)
        {
            lst.Add(new Point(x,y));
        }
    }
}
