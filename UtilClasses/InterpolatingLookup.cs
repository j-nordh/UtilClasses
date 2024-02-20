using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Lists;
using UtilClasses.MathClasses;

namespace UtilClasses
{
    public class InterpolatingLookup
    {
        private List<(float x , float y)> _values =new();

        public void Add(float x, float y)
        {
            _values.Add((x,y));
            _values = _values.OrderBy(x=>x.x).ToList();
        }

        public void AddRange(IEnumerable<(float x, float y)> values)
        {
            _values.AddRange(values);
            _values = _values.OrderBy(x => x.x).ToList();
        }
        
        
        public float Find(float x)
        {
            var p = _values.BinarySearch(x, (t, y2) => t.y.CompareTo(y2));
            if (p > 0) return _values[p].y;

            var v1 = _values[p];
            var v2 = _values[p - 1];
            return MathUtil.Interpolate(v1.x, v1.y, v2.x, v2.y, x);
        }
    }
}
