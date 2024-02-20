using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Types;

namespace UtilClasses
{

    public class BinarySerializer
    {
        private Stream _stream;
        private BinaryWriter _writer;
        private static Dictionary<EnumType, Action<BinaryWriter, object>> _enumSerializers = new();
        private static Dictionary<Type, Action<BinaryWriter, object>> _typeSerializers = new();
        static BinarySerializer()
        {
            AddEnum<byte>(EnumType.Byte, (wr, v) => wr.Write(Convert.ToByte(v)));
            AddEnum<sbyte>(EnumType.SByte, (wr, v) => wr.Write(Convert.ToSByte(v)));
            AddEnum<ushort>(EnumType.UShort, (wr, v) => wr.Write(Convert.ToUInt16(v)));
            AddEnum<short>(EnumType.Short, (wr, v) => wr.Write(Convert.ToInt16(v)));
            AddEnum<uint>(EnumType.UInt, (wr, v) => wr.Write(Convert.ToUInt32(v)));
            AddEnum<int>(EnumType.Int, (wr, v) => wr.Write(Convert.ToInt32(v)));
            AddType<byte>((wr, v) => wr.Write(v));
            AddType<short>((wr, v) => wr.Write(v));
            AddType<ushort>((wr, v) => wr.Write(v));
            AddType<byte[]>((wr, v) => wr.Write(v));
            AddType<string>((wr, v) => wr.Write(v));
            AddType<IPAddress>((wr, a) => wr.Write(a.GetAddressBytes().Reversed().ToArray()));
            AddType<IEnumerable<byte>>((wr, bs) => wr.Write(bs.ToArray()));
        }
        public BinarySerializer(Stream stream, EnumType eType = EnumType.Byte)
        {
            _stream = stream;
            _writer = new BinaryWriter(_stream);
            EnumSerialization = eType;

        }

        public BinarySerializer(byte[] buffer, EnumType eType = EnumType.Byte) : this(new MemoryStream(buffer), eType) { }
        public BinarySerializer(EnumType eType = EnumType.Byte) : this(new MemoryStream(), eType) { }

        public byte[] GetBuffer()
        {
            if (!(_stream is MemoryStream memStream))
                throw new Exception(
                    "You tried to extract a buffer but supplied a stream that's not a memory stream...");
            Flush();
            var ret = new byte[memStream.Length];
            Array.Copy(memStream.GetBuffer(), ret, ret.Length);
            return ret;
        }
        public BinarySerializer Flush()
        {
            _writer.Flush();
            _stream.Flush();
            return this;
        }

        public enum EnumType
        {
            Byte,
            SByte,
            UShort,
            Short,
            UInt,
            Int,
        }
        public EnumType EnumSerialization { get; set; }


        static void AddEnum<T>(EnumType t, Action<BinaryWriter, object> a)
        {
            _enumSerializers[t] = a;
        }
        static void AddType<T>(Action<BinaryWriter, T> a)
        {
            _typeSerializers[typeof(T)] = (wr, v) => a(wr, (T)v);
        }

        public BinarySerializer Write<T>(T val)
        {
            Action f;
            if (typeof(T).IsEnum)
                f = (() => _enumSerializers[EnumSerialization](_writer, val));
            else
            {
                var inner = _typeSerializers.Maybe(typeof(T));
                inner ??= _typeSerializers[_typeSerializers.Keys.First(k => typeof(T).CanBe(k))];
                f = () => inner(_writer, val);
            }

            f();
            return this;
        }
        public BinarySerializer Write<T>(bool predicate, T val) => predicate ? Write(val) : this;
    }

    public class BinaryDeserializer:IDisposable
    {
        private BinaryReader _rdr;
        public BinaryDeserializer(Stream stream)
        {
            _rdr = new(stream);
        }

        public byte ReadByte() => _rdr.ReadByte();
        public byte[] ReadBytes(int c) => _rdr.ReadBytes(c);
        public ushort ReadUInt16() => _rdr.ReadUInt16();

        public void Dispose()
        {
            _rdr?.Dispose();
        }
    }
}
