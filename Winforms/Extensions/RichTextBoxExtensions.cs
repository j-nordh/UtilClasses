using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace UtilClasses.Winforms.Extensions
{
    public static class RichTextBoxExtensions
    {
        public class RichTextRegex
        {
            private readonly RichTextBox _rtxt;
            private readonly Regex _r;
            private const RegexOptions DefaultOptions = RegexOptions.Multiline | RegexOptions.Compiled;
            public RichTextRegex(RichTextBox rtxt, string pattern, RegexOptions? o = null)
            {
                _rtxt = rtxt;
                _r = new Regex(pattern, o?? DefaultOptions);
            }

            public RichTextRegex Regex(string pattern, RegexOptions? o = null) => new RichTextRegex(_rtxt, pattern, o);

            private FontBuilder FB => new FontBuilder(_rtxt.Font);
            public RichTextRegex Color(Color c) => Set(_ => _rtxt.SelectionColor = c);
            public RichTextRegex Bold(bool val=true) => Set(_ => FB.Bold());
            public RichTextRegex Size(float size) => Set(_ => FB.Size(size));

            private RichTextRegex Set(Func<int, FontBuilder> f) => Set(i => _rtxt.SelectionFont = f(i).Build());
            private RichTextRegex Set( Action<int> a)
            {
                foreach (var m in _r.Matches(_rtxt.Text).OfType<Match>())
                {
                    var count = 0;
                    bool skipped = false;
                    foreach (var g in m.Groups.OfType<Group>())
                    {
                        if (m.Groups.Count > 1 && !skipped)
                        {
                            skipped = true;
                            continue;
                        }
                        _rtxt.SelectionStart = g.Index;
                        _rtxt.SelectionLength = g.Length;
                        a(count);
                        count += 1;
                    }
                }
                return this;
            }
        }

        public static RichTextRegex Regex(this RichTextBox rtxt, string pattern) => new RichTextRegex(rtxt, pattern);
    }

}
