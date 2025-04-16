using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilClasses.Winforms.Extensions
{
    public static class Layout
    {
        public static Point TopLeft(this Padding p) => new Point(p.Left, p.Top);

        public static Padding WithTop(this Padding p, int top)=>new Padding(p.Left,top, p.Right, p.Bottom);
    }
}
