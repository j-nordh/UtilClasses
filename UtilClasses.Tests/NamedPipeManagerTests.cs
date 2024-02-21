using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using UtilClasses.Interfaces;
using UtilClasses.Dataflow;
using UtilClasses.Extensions.Bytes;
using Xunit;
using Xunit.Abstractions;

namespace UtilClasses.Tests
{
    public class NamedPipeManagerTests
    {
        private readonly ITestOutputHelper _output;

        public NamedPipeManagerTests(ITestOutputHelper output)
        {
            _output = output;
        }
        //[Fact]
        //public async Task StartStop()
        //{
        //    _output.WriteLine("StartStop: Starting");
        //    var pipes = new Pipes<byte>("startStopTest");

        //    _output.WriteLine("StartStop: Calling start");
        //    await pipes.Start();
        //    _output.WriteLine("StartStop: Calling stop");
        //    pipes.Stop();
        //    _output.WriteLine("StartStop: Done");

        //    Assert.True(true);
        //}



        //[Theory]
        //[InlineData(0)]
        //[InlineData(1)]
        //[InlineData(67)]
        //[InlineData(254)]
        //[InlineData(255)]
        //public async Task ByteReadBack(byte b) => await ReadBack(b);

        //[Theory]
        //[InlineData(0)]
        //[InlineData(1)]
        //[InlineData(215699875)]
        //[InlineData(long.MaxValue)]
        //[InlineData(long.MaxValue - 1)]
        //[InlineData(long.MinValue + 1)]
        //[InlineData(long.MinValue)]
        //public async Task LongReadBack(long v) => await ReadBack(v);

        //private async Task ReadBack<T>(T v)
        //{
        //    await OnPipes<T>(async pipes => await pipes.ReadBack(v));
        //}



        //[Fact]
        //public async Task MultiBlockBytes()
        //{
        //    await OnPipes<byte>(async pipes =>
        //    {
        //        for (int i = 0; i < 10000; i++)
        //        {
        //            var b = (byte) (i % 255);
        //            await pipes.ReadBack(b);
        //        }
        //    });
        //}

        //private async Task OnPipes<T>(Action<Pipes<T>> a) => await OnPipes(0, a);
        //private async Task OnPipes<T>(int length, Action<Pipes<T>> a)
        //{
        //    var pipes = new Pipes<T>("ReadBackTest");
        //    await pipes.Start();
        //    a(pipes);
        //    pipes.Stop();
        //    Assert.True(true);
        //}

        //public class Pipes<T> : IDisposable
        //{
        //    private static readonly Dictionary<Type, Func<object>> _packerLookup;

        //    private static void AddPacker<TObj>(Func<IBytePacker<TObj>> f) => _packerLookup[typeof(TObj)] = f;
        //    private static IBytePacker<TObj> GetPacker<TObj>() => _packerLookup[typeof(TObj)]() as IBytePacker<TObj>;
        //    static Pipes()
        //    {
        //        _packerLookup = new Dictionary<Type, Func<object>>();
        //        AddPacker(() => new BytePacker());
        //        AddPacker(()=>new LongPacker());
        //    }
        //    public NamedPipeServer<T, IBytePacker<T>> Server { get; }
        //    public NamedPipeClient<T, IBytePacker<T>> Client { get; }

        //    public Pipes(string name)
        //    {
        //        var packer = GetPacker<T>();
        //        Server = new NamedPipeServer<T, IBytePacker<T>>(name, packer);
        //        Client = new NamedPipeClient<T, IBytePacker<T>>(name, packer);
        //    }
        //    public async Task Start()
        //    {
        //        await Server.SetupRead();
        //        Server.StartRead();
        //        await Client.SetupWrite();
        //    }

        //    public void Stop()
        //    {
        //        Server.StopRead();
        //    }

        //    public void Dispose()
        //    {
        //        Server?.Dispose();
        //        Client?.Dispose();
        //    }
        //}

        //class LongPacker : IBytePacker<long>
        //{
        //    private static readonly int Size;
        //    private static readonly int DataSize;

        //    static LongPacker()
        //    {
        //        Size = sizeof(long) + 1;
        //        DataSize = sizeof(long);
        //    }

        //    public int Pack(long obj, byte[] buffer, int offset)
        //    {
        //        BitConverter.GetBytes(obj).CopyTo(buffer, offset);
        //        buffer[offset + DataSize] = buffer.GetCheckByte(offset, DataSize);
        //        return DataSize;
        //    }

        //    public UnpackResult Unpack(byte[] buffer, int offset, out long obj)
        //    {
        //        obj = 0;
        //        if (buffer.Length < offset + Size) return UnpackResult.NotEnoughData;
        //        if (0 != buffer.GetCheckByte(offset, Size)) return UnpackResult.Corrupt;
        //        obj = BitConverter.ToInt64(buffer, offset);
        //        return UnpackResult.Ok;
        //    }

        //    public int ObjectSize => Size;
        //}
        //class BytePacker : IBytePacker<byte>
        //{
        //    public int Pack(byte obj, byte[] buffer, int offset)
        //    {
        //        buffer[offset] = obj;
        //        return 1;
        //    }

        //    public UnpackResult Unpack(byte[] buffer, int offset, out byte obj)
        //    {
        //        obj = buffer[offset];
        //        return UnpackResult.Ok;
        //    }

        //    public int ObjectSize => 1;
        //}
    }

    //static class PipesExtensions
    //{
    //    public static async Task ReadBack<T>(this NamedPipeManagerTests.Pipes<T> pipes, T v)
    //    {
    //        await pipes.Client.Write(v);
    //        var r = await pipes.Server.OutBlock.ReceiveAsync();
    //        Assert.Equal(v, r);
    //    }
    //}
}
