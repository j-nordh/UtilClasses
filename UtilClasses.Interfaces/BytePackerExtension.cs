using System.Linq;

namespace UtilClasses.Interfaces
{
    public static class BytePackerExtension
    {
        public static byte[] Pack<T>(this IBytePacker<T> packer, T o)
        {
            var len = packer.ObjectSize > 0 ? packer.ObjectSize : packer.CalculatedObjectSize(o);
            var buff = new byte[len];
            packer.Pack(o, buff, 0);
            return buff;
        }

        public static byte[] Pack<T>(this IBytePacker<T> packer, T[] os)
        {
            var lst = os.ToList();
            var size = packer.ObjectSize * lst.Count;
            var buff = new byte[size];
            lst.Aggregate(0, (current, obj) =>
            {
                packer.Pack(obj, buff, current);
                return current + packer.ObjectSize;
            });
            return buff;
        }


    }
}