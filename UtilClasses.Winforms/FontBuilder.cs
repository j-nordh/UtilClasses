using System.Drawing;
using UtilClasses.Extensions.Objects;

namespace UtilClasses.Winforms
{
    public class FontBuilder
    {
        private FontFamily _family;
        private float _size;
        private FontStyle _style;
        private GraphicsUnit _gu;
        private byte _charset;
        private bool _verticalFont;
        public FontBuilder(Font prototype)
        {
            _family = prototype.FontFamily;
            _size = prototype.Size;
            _style = prototype.Style;
            _gu = prototype.Unit;
            _charset = prototype.GdiCharSet;
            _verticalFont = prototype.GdiVerticalFont;
        }

        public FontBuilder Size(float size) => this.Do(()=>_size = size);

        public FontBuilder Bold(bool val = true)
        {
            if (val)
                _style |= FontStyle.Bold;
            else
                _style &= ~FontStyle.Bold;
            return this;
        }

        public Font Build() => new Font(_family, _size, _style,  _gu, _charset, _verticalFont);
    }
}
