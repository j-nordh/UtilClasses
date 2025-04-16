using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;

namespace UtilClasses.Winforms.Splash
{
    public partial class SplashForm : Form
    {
        private readonly SplashOwner _owner;
        private readonly Bitmap _orgImg;
        private readonly Bitmap _img;
        private readonly IntPtr _memDc;
        private readonly IntPtr _hBmp;
        private readonly IntPtr _hOldBmp;
        private Rectangle _lastMessageRect;
        private int _clickCount;
        public enum SplashOwner
        {
            Combi,
            Complete,
            Maintenance
        }


        public SplashForm(Bitmap img, SplashOwner owner, bool debug=false)
        {
            _owner = owner;
            InitializeComponent();
            
            _img = img.Clone(new Rectangle(new Point(0,0), img.Size),img.PixelFormat);
            var version = "";
            var rev = "";
            var ver = Assembly.GetEntryAssembly().GetName().Version;
            if (ver.Revision > 0)
            {
                version = "BETA  ";
                rev = $".{ver.Revision}";
            }
            version += $"{ver.Major}.{ver.Minor}.{ver.Build}{rev}";
            var font = new Font("Arial", 12);
            

            using (var g = Graphics.FromImage(_img))
            {
                var size = TextRenderer.MeasureText(g,version, font);
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                TextRenderer.DrawText(g, version, font, GetVersionPos(size), Color.FromArgb(8, 32, 107));
            }
            _orgImg = _img.Clone(new Rectangle(new Point(0, 0), _img.Size), _img.PixelFormat);
            Width = img.Width;
            Height = img.Height;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;
            Resize += OnResize;

            var screenDc = ApiHelp.GetDC(IntPtr.Zero);

            _memDc = ApiHelp.CreateCompatibleDC(screenDc);
            _hBmp = _img.GetHbitmap(Color.FromArgb(0));
            _hOldBmp = ApiHelp.SelectObject(_memDc, _hBmp); //memDc is a device context that contains our image

            ApiHelp.DeleteDC(screenDc);
            if (debug)
            {
                Click += SplashForm_Click;
                KeyDown += SplashForm_KeyDown;
            }
        }

        private void SplashForm_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Escape) Close();
        }

        private void SplashForm_Click(object sender, EventArgs e)
        {
            var msgs = new List<string>
            {
                "A short message",
                "A longer message",
                "An even longer message",
                "A very long, but still quite plausible, message",
                "A ridiculously long message, too long to ever fit, but a message all the same"
            };
            Message = msgs[_clickCount % msgs.Count];
            _clickCount += 1;
        }

        private void OnResize(object sender, EventArgs e)
        {
            ReDraw();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();

            }
            ApiHelp.SelectObject(_memDc, _hOldBmp);
            ApiHelp.DeleteObject(_hBmp);
            ApiHelp.DeleteDC(_memDc);
            base.Dispose(disposing);
        }

        public string Message
        {
            set
            {
                using (var g = Graphics.FromHdc(_memDc))
                {
                    var font = new Font("Arial", 9);
                    var size = TextRenderer.MeasureText(g, value, font);
                    var pos = GetMessagePos(size);
                    if (Rectangle.Empty != _lastMessageRect)
                    {
                        g.DrawImage(_orgImg, _lastMessageRect, _lastMessageRect, GraphicsUnit.Pixel);
                    }
                    using (var backBuffer = new Bitmap(_img, size))
                    {
                        using (var backBufferGraphics = Graphics.FromImage(backBuffer))
                        {
                            backBufferGraphics.DrawImage(
                                _img, 
                                new Rectangle(Point.Empty, size),
                                new Rectangle(pos, size), 
                                GraphicsUnit.Pixel);

                            backBufferGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                            TextRenderer.DrawText(backBufferGraphics, value, font, Point.Empty, Color.Black);
                        }
                        g.DrawImage(
                            backBuffer, 
                            new Rectangle(pos, size), 
                            new Rectangle(Point.Empty, size),
                            GraphicsUnit.Pixel);
                    }
                    _lastMessageRect = new Rectangle(pos, size);
                }
                ReDraw();
            }
        }

        private Point GetMessagePos(Size size)
        {
            switch (_owner)
            {
                case SplashOwner.Complete:
                    return new Point(10, 232);
                case SplashOwner.Maintenance:
                    return new Point((Width-size.Width)/2, 380-size.Height);
                case SplashOwner.Combi:
                    return new Point(634 - size.Width, 232);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Point GetVersionPos(Size size) => new Point(615 - size.Width, 60);

        private void SplashForm_Load(object sender, EventArgs e)
        {
            ReDraw();
        }

        private void ReDraw()
        {
            ApiHelp.BLENDFUNCTION blend;
            //Only works with a 32bpp bitmap
            blend.BlendOp = ApiHelp.AC_SRC_OVER;
            //Always 0
            blend.BlendFlags = 0;
            //Set to 255 for per-pixel alpha values
            blend.SourceConstantAlpha = 255;
            //Only works when the bitmap contains an alpha channel
            blend.AlphaFormat = ApiHelp.AC_SRC_ALPHA;

            ApiHelp.Size newSize;
            ApiHelp.Point newLocation;
            ApiHelp.Point sourceLocation;

            newLocation.x = Location.X;
            newLocation.y = Location.Y;

            sourceLocation.x = 0;
            sourceLocation.y = 0;

            newSize.cx = Width;
            newSize.cy = Height;

            ApiHelp.UpdateLayeredWindow(Handle, IntPtr.Zero, ref newLocation, ref newSize, _memDc, ref sourceLocation, 0, ref blend, ApiHelp.ULW_ALPHA);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // Add the layered extended style (WS_EX_LAYERED) to this window
                CreateParams createParam = base.CreateParams;
                createParam.ExStyle = (createParam.ExStyle | ApiHelp.WS_EX_LAYERED);
                return createParam;
            }
        }
    }
}
