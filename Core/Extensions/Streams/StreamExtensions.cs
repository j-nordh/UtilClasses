using System;
using System.IO;
using System.Text;
using UtilClasses.Core.Extensions.DateTimes;
using UtilClasses.Core.Extensions.Objects;

namespace UtilClasses.Core.Extensions.Streams;

public static class StreamExtensions
{
    public static string AsString(this Stream s, Encoding? enc = null, bool disposeStream=true, bool rewind=true)
    {
        if(null==enc)enc = Encoding.UTF8;
        string ret;
        if (rewind)
            s.Position = 0;
        using (var rdr = new StreamReader(s, enc))
        {
            ret =  rdr.ReadToEnd();
        }
        if(disposeStream)
            s.Dispose();
        return ret;
    }

    public static Stream Write(this Stream s, string str, Encoding enc, bool leaveOpen = false )
    {
        using (var wr = new StreamWriter(s, enc,1024,leaveOpen))
            wr.Write(str);
        return s;
    }

    public static Stream WriteUtf8(this Stream s, string str) => Write(s, str, Encoding.UTF8);
    public static Stream LogUtf8(this Stream s, string str, Encoding? enc = null) => Write(s, $"{DateTime.UtcNow.ToSaneString()}: {str}\n", enc ?? Encoding.UTF8, true);

    public static BinaryWriter Wr(this BinaryWriter wr, byte val) => wr.Do(() => wr.Write(val));
    public static BinaryWriter Wr(this BinaryWriter wr, short val) => wr.Do(() => wr.Write(val));
    public static BinaryWriter Wr(this BinaryWriter wr, string val) => wr.Do(() => wr.Write(val));
    public static BinaryWriter Wr(this BinaryWriter wr, ushort val) => wr.Do(() => wr.Write(val));
    public static BinaryWriter Wr(this BinaryWriter wr, byte[] val) => wr.Do(() => wr.Write(val));
}