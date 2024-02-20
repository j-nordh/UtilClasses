using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilClasses.Winforms
{
    public class CueTextBox : TextBox
    {

        string _cue;
        public string Cue
        {
            get { return _cue; }
            set
            {
                _cue = value;
                OnHintChanged();
            }
        }

        protected virtual void OnHintChanged()
        {
            SendMessage(this.Handle, EM_SETCUEBANNER, 1, _cue);
        }

        const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam,
            [MarshalAs(UnmanagedType.LPWStr)] string lParam);

    }
}
