using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Geometry
{
    public class Size
    {
        public int Width { get; }
        public int Height { get; }
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
    public class SizeD
    {
        public double Width { get; }
        public double Height { get; }
        public SizeD(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }
}
