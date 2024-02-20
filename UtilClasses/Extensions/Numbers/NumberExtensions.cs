using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Extensions.Numbers
{
    public static  class NumberExtensions
    {
        public static double Squared(this double d) => d * d;
        public static double ToRad(this double d) => d*Math.PI/180;
        public static double FromRad(this double d) => d*180/Math.PI;
    }
}
