using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UtilClasses.Core;

public class BitStream : IEquatable<BitStream>
{
    private readonly bool _bigEndian;
    private readonly BitArray? _bits;
    public int WritePosition { get; private set; }
    public int ReadPosition { get; private set; }
    public FirstBitLocation FirstBit { get; }
    private readonly EndianBitConverter _ebc;

    private BitStream(FirstBitLocation fbl, bool bigEndian)
    {
        _bigEndian = bigEndian;
        FirstBit = fbl;
        _ebc = new EndianBitConverter(bigEndian);
            
    }
    public BitStream(byte[] bytes, FirstBitLocation firstBit, bool bigEndian) : this(firstBit, bigEndian)
    {
        _bits = new BitArray(bytes);
        var totalBits = bytes.Length * 8;
        WritePosition = firstBit > 0 ? totalBits - 1 : 0;
        ReadPosition = totalBits - WritePosition;
    }

    public BitStream(int length, FirstBitLocation firstBit, bool bigEndian) : this(firstBit, bigEndian)
    {
        _bits = new BitArray(length);
        WritePosition = ReadPosition = firstBit > 0 ? 0 : length - 1;
    }

    public BitStream WriteBytes(byte[] val, int bitsPerByte)
    {
        try
        {
            for (int i = 0; i < val.Length; i++)
            {
                WriteByte(val[i], bitsPerByte);
            }
            //val.ForEach(b => WriteByte(b, bitsPerByte));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return this;
    }

    public BitStream WriteBits(byte[] bytes, int bits, bool isWord)
    {
        if (bits > bytes.Length * 8) throw new ArgumentOutOfRangeException();
        var byteCount = (int) Math.Ceiling(bits / 8.0);
        if (!isWord)
        {
            if(bits * 8 != bytes.Length)
                throw new ArgumentException("Only words may be written with less bits than the array contains");

            for (int i = 0; i < byteCount; i++)
                WriteByte(bytes[i], 8);
            return this;
        }

        if (bytes.Length > byteCount)
        {
            var index = _bigEndian ? bytes.Length - byteCount : 0;
            bytes = bytes.Skip(index).Take(byteCount).ToArray();
        }


        int byteIndex = 0;
        while (bits > 0)
        {
            var bitsToWrite = _bigEndian ? Math.Min(8, bits): bits % 8;
            if (bitsToWrite == 0) bitsToWrite = 8;
            WriteByte(bytes[byteIndex], bitsToWrite);
            bits -= bitsToWrite;
            byteIndex += 1;
        }
        return this;
    }


    public byte[] ReadBytes(int bits, int bitsPerByte = 8)
    {
        var byteCount = (int)Math.Ceiling(bits * 1.0 / bitsPerByte);
        var ret = new byte[byteCount];
        for (int i = 0; i < byteCount; i++)
        {
            ret[i] = ReadByte(bitsPerByte);
        }

        return ret;
    }
    public BitStream WriteByte(byte val, int bits)
    {
        try
        {
            for (int j = 0; j < bits; j++)
                switch (FirstBit)
                {
                    case FirstBitLocation.FirstByteFirstBit:
                    case FirstBitLocation.LastByteLastBit:
                        WriteBit(val, j);
                        break;
                    case FirstBitLocation.FirstByteLastBit:
                    case FirstBitLocation.LastByteFirstBit:
                        WriteBit(val, bits - j - 1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return this;
    }

    public byte ReadByte(int bitsPerByte = 8)
    {
        byte ret = 0;
        for (int j = 0; j < bitsPerByte; j++)
        {
            switch (FirstBit)
            {
                case FirstBitLocation.FirstByteFirstBit:
                case FirstBitLocation.LastByteLastBit:
                    ReadBit(ref ret, j);
                    break;
                case FirstBitLocation.LastByteFirstBit:
                case FirstBitLocation.FirstByteLastBit:
                    ReadBit(ref ret, bitsPerByte - j - 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        return ret;
    }

    public void WriteBit(byte val, int index) => WriteBit((val & 1 << index) > 0);
    public void WriteBit(bool value)
    {
        if (null == _bits)
            throw new Exception("BitStream not initialized");
        _bits[FirstBit > 0 ? WritePosition++ : WritePosition--] = value;
    }

    public bool ReadBit()
    {
        if (null == _bits)
            throw new Exception("BitStream not initialized");
        return _bits[FirstBit > 0 ? ReadPosition++ : ReadPosition--];
    }

    public byte ReadBit(byte current, int index) => (byte)(current | (ReadBit() ? 1 << index : 0));
    public void ReadBit(ref byte b, int index) => b = ReadBit(b, index);

    public int GetBufferIndex(int i)
    {
        var byteIndex = i / 8;
        switch (FirstBit)
        {
            case FirstBitLocation.LastByteFirstBit:
                var bytes = (int)Math.Ceiling(_bits.Length / 8.0);
                return bytes - byteIndex + i % 8;
            case FirstBitLocation.LastByteLastBit:
                return _bits.Length - i;
            case FirstBitLocation.FirstByteFirstBit:
                return i;
            case FirstBitLocation.FirstByteLastBit:
                return (byteIndex + 1) * 8 - i % 8;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public BitArray ReadBits(int nbrOfBits)
    {
        var ret = new BitArray(nbrOfBits);

        return ReadBits(ret);
    }

    public BitArray ReadBits(BitArray arr)
    {
        for (var i = 0; i < arr.Count; i++)
        {
            var bit = this.ReadBit();
            arr.Set(i, bit);
        }

        return arr;
    }
    public bool CanRead() => FirstBit > 0 ? ReadPosition < WritePosition : ReadPosition > WritePosition;

    public byte[] ReadWhile(Func<byte[], bool> isEndMarker, int bitsPerChar = 8, int maxBits = int.MaxValue)
    {
        var lst = new List<byte>();
        var count = 0;
        while (count < maxBits && CanRead())
        {
            var b = ReadBytes(bitsPerChar, Math.Min(8, bitsPerChar));
            count += bitsPerChar;
            if (isEndMarker(b))
                break;
            lst.AddRange(b);
        }

        return lst.ToArray();
    }

    public byte[] GetBytes(bool truncate = false)
    {
        int byteCount;
        byte[] ret;
        if (truncate)
        {
            var actualBits = FirstBit > 0
                ? WritePosition
                : _bits.Length - WritePosition;
            byteCount = (int)Math.Ceiling(actualBits / 8.0);
            var tmpRead = ReadPosition;
            var tmpWrite = WritePosition;
            ReadPosition = FirstBit > 0 ? 0 : _bits.Length;
            WritePosition = _bits.Length - ReadPosition;
            ret = ReadBytes(actualBits);
            ReadPosition = tmpRead;
            WritePosition = tmpWrite;
            return ret;
        }
        byteCount = (int)Math.Ceiling(_bits.Length / 8.0);
        ret = new byte[byteCount];
        _bits.CopyTo(ret, 0);
        return ret;
    }

    public BitStream Write(int val, int bits = 32) => WriteBits(_ebc.GetBytes(val), bits, true);

        
    public BitStream Write(string val, Encoding enc, int bitsPerChar = 8)
    {
        var bs = enc.GetBytes(val);
        var bitsPerByte = Math.Min(bitsPerChar, 8);
        WriteBytes(bs, bitsPerByte);
        return this;
    }

    public int ReadInt(int bits = 32)
    {
        var bytes = ReadBytes(bits);
        return _ebc.ToInt32(bytes, 0);
    }

    public float ReadFloat(int length)
    {
        throw new NotImplementedException();
    }

    public void SkipRead(int length) => ReadPosition += length;
    public void SkipWrite(int length) => WritePosition += length;
    public void ResetReadPos() => ReadPosition = 0;
    public void ResetWritePos() => WritePosition = 0;

    public void Clear()
    {
        _bits.SetAll(false);
        ResetReadPos();
        ResetWritePos();
    }

    public string ReadString(Encoding enc, int bitsPerChar)
    {
        return ReadString(enc.GetString, bitsPerChar, int.MaxValue, true);
    }
    public string ReadString(Encoding enc, int bitsPerChar, int length)
    {
        return ReadString(enc.GetString, bitsPerChar, length, false);
    }


    protected string ReadString(Func<byte[], string> resolver, int bitsPerChar, int length, bool endOnNull)
    {
        if (length * bitsPerChar > _bits.Length && !endOnNull)
            throw new ArgumentException(
                "You want to read more characters than the buffer contains and won't end on null. Are you mad?");
        var bytes = endOnNull
            ? ReadWhile(bs => bs.All(b => b == 0), bitsPerChar)
            : ReadBytes(bitsPerChar * length, Math.Min(bitsPerChar, 8));
        return resolver(bytes);
    }

    public char ReadChar(int length)
    {
        throw new NotImplementedException();
    }

    public DateTime ReadDate(int length)
    {
        throw new NotImplementedException();
    }

    public T ReadEnum<T>(int length)
    {
        throw new NotImplementedException();
    }


    public bool Equals(BitStream other)
    {
        if (null == other) return false;
        if (FirstBit != other.FirstBit) return false;
        if (_bits.Length != other._bits.Length) return false;
        for (int i = 0; i < _bits.Length; i++)
        {
            if (_bits[i] != other._bits[i]) return false;
        }
        return true;
    }
}
public enum FirstBitLocation
{
    LastByteFirstBit = -1,
    LastByteLastBit = -2,
    FirstByteFirstBit = 1,
    FirstByteLastBit = 2
}

static class BitStreamExtensions
{
    public static char GetChar(this Encoding enc, byte[] bs) => enc.GetChars(bs, 0, bs.Length).First();
}