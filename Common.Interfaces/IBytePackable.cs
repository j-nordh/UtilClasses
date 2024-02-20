using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Interfaces
{
    public interface IBytePackable
    {
        void Pack(byte[] buffer, int offset);
        int PackedSize { get; }
    }

    public enum UnpackResult
    {
        Ok,
        Corrupt,
        NotEnoughData
    }
    public interface IBytePacker<T>
    {
        int Pack(T obj, byte[] buffer, int offset);
        UnpackResult Unpack(byte[] buffer, int offset, out T obj);
        T Unpack(byte[] buffer);
        public T[] UnpackAll(byte[] buffer);
        public int ObjectSize { get; }
        public int CalculatedObjectSize(T obj);
    }

    public interface IBytePacker<T, TSetup> : IBytePacker<T>
    {
        public void Setup(TSetup channels);
    }

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