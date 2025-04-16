using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilClasses.Winforms
{
    public class TemporaryCursor: IDisposable
    {
        private readonly Cursor _prev;

        public TemporaryCursor(Cursor c)
        {
            _prev = Cursor.Current;
            Cursor.Current = c;
        }

        public void Dispose()
        {
            Cursor.Current = _prev;
        }
    }

    public class TemporaryDisable :IDisposable
    {
        private readonly List<Control> _controls;
        private readonly bool _enabled;

        public TemporaryDisable(IEnumerable<Control> controls, bool enabled)
        {
            _controls = new List<Control>(controls);
            _controls.ForEach(c => c.Enabled = enabled);
            _enabled = enabled;
        }
        public void Dispose()
        {
            _controls.ForEach(c => c.Enabled = !_enabled);
        }
    }

    public class Temporary
    {
        public static IDisposable Cursor(Cursor c) => new TemporaryCursor(c);
        public static IDisposable Disable(params Control[] controls) => new TemporaryDisable(controls, false);
    }
}
