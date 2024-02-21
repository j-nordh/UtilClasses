using System;
using System.Collections.Generic;

namespace UtilClasses.Interfaces
{
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
        T[] UnpackAll(byte[] buffer);
        int ObjectSize { get; }
        int CalculatedObjectSize(T obj);
    }

    public interface IBytePacker<T, TSetup> : IBytePacker<T>
    {
        void Setup(TSetup channels);
    }
}