using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace UtilClasses.Winforms
{
    public class DirectBitmap : IDisposable
    {
        public byte[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new byte[width * height*4];
            
        }

        public static DirectBitmap FromBitmap(Bitmap bmp)
        {
            var ret = new DirectBitmap(bmp.Width, bmp.Height);
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                    ret.SetPixel(x, y, bmp.GetPixel(x, y));
            return ret;
        }

        public void SetPixel(int x, int y, Color colour) => SetPixel(GetIndex(x, y), colour);

        public void SetPixel(int index, Color col)
        {
            Bits[index] = col.A;
            Bits[index + 1] = col.R;
            Bits[index + 2] = col.G;
            Bits[index + 3] = col.B;
        }

        private int GetIndex(int x, int y) =>(x + (y* Width))*4;

        public Color GetPixel(int i)
         => Color.FromArgb(Bits[i], Bits[i + 1], Bits[i + 2], Bits[i + 3]);
        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(Bits[index]);

            return result;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
        }

        public BitmapHolder GetHolder()=> new BitmapHolder(Width, Height,Bits);

        public class BitmapHolder:IDisposable
        {
            public Bitmap Bitmap { get; private set; }
            public byte[] Bits { get; private set; }
            public bool Disposed { get; private set; }
            protected GCHandle BitsHandle { get; private set; }

            public BitmapHolder(int width, int height, byte[]bits)
            {
                Bits = new byte[bits.Length];
                bits.CopyTo(Bits, 0);
                BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
                Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
            }

            public void Dispose()
            {
                if (Disposed) return;
                Disposed = true;
                Bitmap.Dispose();
                BitsHandle.Free();
            }
        }
        public DirectBitmap Merge(DirectBitmap other, Func<byte,byte,byte> f)
        {
            if (Width != other.Width) throw new Exception("This image is wider than the other");
            if (Height != other.Height) throw new Exception("This image is taller than the othter");
            for (int i = 0; i < Bits.Length; i++)
                Bits[i] = f(Bits[i], other.Bits[i]);
            return this;
        }
    }
}
