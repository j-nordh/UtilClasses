using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IStateful
    {
        int OriginalStateCode { get; }
        int GetStateCode();
        void MarkAsClean();
    }
    public static class StatefulExtensions
    {
        public static bool IsDirty(this IStateful o)
        {
            if (null == o) return false;
            return o.GetStateCode() != o.OriginalStateCode;
        }

        public static IEnumerable<T> Dirty<T>(this IEnumerable<T> items) where T : IStateful =>
            items.Where(i => i.IsDirty());
        public static void WhenDirty<T>(this T o, Action<T> a) where T : IStateful
        {
            if(o.GetStateCode() == o.OriginalStateCode) return;
            a(o);
        }
        public static T WhenDirty<T>(this T o, Func<T,T> a) where T : IStateful
        {
            if (o.GetStateCode() == o.OriginalStateCode) return o;
            return a(o);
        }
        public static T AsClean<T>(this T o) where T:IStateful
        {
            o?.MarkAsClean();
            return o;
        }

        public static IEnumerable<T> AsClean<T>(this IEnumerable<T> os) where T : IStateful =>
            os?.Select(o => o.AsClean());

    }
        

}
