using System;

namespace UtilClasses.Core;

public class EndianBitConverter
{
    private bool _changeOrder;
    public EndianBitConverter(bool big)
    {
        _changeOrder = big == BitConverter.IsLittleEndian;
    }
    public int ToInt32(byte[] bytes, int offset) => ToT(BitConverter.ToInt32, bytes, offset, _changeOrder, 4);
    public double ToDouble(byte[] bytes, int offset) => ToT(BitConverter.ToDouble, bytes, offset, _changeOrder, 8);
    public float ToSingle(byte[] bytes, int offset) => ToT(BitConverter.ToSingle, bytes, offset, _changeOrder, 4);
    public float ToFloat(byte[] bytes, int offset) => ToSingle(bytes, offset);

    public static int ToInt32(byte[] bytes, int offset, bool bigEndian) => ToT(BitConverter.ToInt32, bytes, offset, BitConverter.IsLittleEndian == bigEndian, 4);
    public static int ToInt32(byte[] bytes, bool bigEndian) => ToT(BitConverter.ToInt32, bytes, 0, BitConverter.IsLittleEndian == bigEndian, 4);
    public static double ToDouble(byte[] bytes, int offset, bool bigEndian) => ToT(BitConverter.ToDouble, bytes, offset, BitConverter.IsLittleEndian == bigEndian, 8);
    public static float ToSingle(byte[] bytes, int offset, bool bigEndian) => ToT(BitConverter.ToSingle, bytes, offset, BitConverter.IsLittleEndian == bigEndian, 4);
    public static float ToFloat(byte[] bytes, int offset, bool bigEndian) => ToSingle(bytes, offset, BitConverter.IsLittleEndian == bigEndian);

    public byte[] GetBytes(int i) => GetBytes(BitConverter.GetBytes, i, _changeOrder, 4);
    public static byte[] GetBytes(int i, bool bigEndian) => GetBytes(BitConverter.GetBytes, i, BitConverter.IsLittleEndian == bigEndian, 4);
    public static void CopyBytes<T>(T value, byte[] targetBytes, int targetOffset, Func<T, byte[]> f, bool bigEndian)
    {
        var val = GetBytes<T>(f, value, BitConverter.IsLittleEndian == bigEndian, 4);
        for (int i = 0; i < val.Length; i++)
        {
            targetBytes[targetOffset + i] = val[i];
        }
    }

    public static void CopyBytes(int value, byte[] target, int offset, bool bigEndian) =>
        CopyBytes(value, target, offset, BitConverter.GetBytes, bigEndian);

    private static T ToT<T>(Func<byte[], int, T> f, byte[] bytes, int offset, bool changeOrder, int requiredLength)
    {
        var length = Math.Min(bytes.Length - offset, requiredLength);
        var newBytes = new byte[requiredLength];
        Array.Copy(bytes, offset, newBytes, 0, length);
        return changeOrder
            ? f(TakeAndReverse(newBytes, 0, length, requiredLength), 0)
            : f(newBytes, 0);
    }

    private static byte[] GetBytes<T>(Func<T, byte[]> f, T o, bool changeOrder, int requiredLength) => changeOrder
        ? TakeAndReverse(f(o), 0, requiredLength, requiredLength)
        : f(o);

    private static byte[] TakeAndReverse(byte[] bytes, int offset, int length, int outLength)
    {
        if (length < 0) length = bytes.Length - offset;
        if (bytes.Length < offset + length) throw new ArgumentOutOfRangeException();
        var ret = new byte[outLength];
        for (var i = 0; i < length; i++)
        {
            ret[length - i - 1] = bytes[offset + i];
        }
        return ret;
    }
}