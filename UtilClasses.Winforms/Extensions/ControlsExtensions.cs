using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilClasses.Winforms.Extensions
{
    public static class ControlsExtensions
    {
        public static Control Find(this IEnumerable<Control> ctrls, Predicate<Control> predicate)
        {
            foreach (var ctrl in ctrls)
            {
                if (predicate(ctrl)) return ctrl;
                var res = ctrl.Controls.Cast<Control>().Find(predicate);
                if (null != res) return res;
            }
            return null;
        }

        public static Control Find(this Control.ControlCollection ctrls, Predicate<Control> predicate)
        {
            return ctrls.Cast<Control>().Find(predicate);
        }

        public static Control FindParent(this Control ctrl, Predicate<Control> predicate)
        {
            if (ctrl.Parent == null) return null;
            if (predicate(ctrl.Parent)) return ctrl.Parent;
            return ctrl.Parent.FindParent(predicate);
        }

        public static T FindParent<T>(this Control ctrl) where T:Control
        {
            return (T)FindParent(ctrl, p => (p as T) != null);
        }

        public static void InvokeAndSetText(this Control ctrl, string txt)
        {
            if (ctrl.InvokeRequired)
                ctrl.Invoke(new Action(() => ctrl.Text = txt));
            else
                ctrl.Text = txt;
        }

        public static void InvokeAndSet(this ProgressBar ctrl, int value)
            => ctrl.InvokeIfNeeded(pb => pb.Value = value);

        public static void InvokeAndIncrement(this ProgressBar ctrl, int value = 1) =>
            ctrl.InvokeIfNeeded(pb => pb.Increment(value));

        public static void Invoke_Enable(this Control ctrl, bool enabled =true)
        {
            if (ctrl.InvokeRequired)
                ctrl.Invoke(new Action(() => ctrl.Enabled =enabled));
            else
                ctrl.Enabled = enabled;
        }
        public static void Invoke_Disable(this Control ctrl)
        {
            if (ctrl.InvokeRequired)
                ctrl.Invoke(new Action(() => ctrl.Enabled = false));
            else
                ctrl.Enabled = false;
        }

        public static void InvokeSetTextScrollToEnd(this TextBoxBase txt, string s)
        {
            var a = new Action(() =>
            {
                txt.Text = s;
                txt.SelectionStart = s.Length;
                txt.SelectionLength = 0;
                txt.ScrollToCaret();
            });
            if (txt.InvokeRequired) txt.Invoke(a);
            else a();
        }

        public static void Invoke(this Control control, Action action)
        {
            control.Invoke((Delegate)action);
        }

        public static void Invoke<T>(this T control, Action<T> action) where T:Control
        {
            control.Invoke(()=>action(control));
        }
        public static void Invoke(this Panel p, Action action)
        {
            p.Invoke(action);
        }

        public static void InvokeIfNeeded(this Control control, Action action)
        {
            if (control.InvokeRequired) control.Invoke(action);
            else action();
        }
        public static void InvokeIfNeeded<T>(this T control, Action<T> action) where T:Control
        {
            if (control.InvokeRequired) control.Invoke(()=>action(control));
            else action(control);
        }


        public static T FindActive<T>(this ContainerControl c) where T : class
        {
            while (true)
            {
                if (c?.ActiveControl == null) return null;

                var ret = c.ActiveControl as T;
                if (ret != null) return ret;

                c = c.ActiveControl as ContainerControl;
            }
        }

        public static CheckState CombineCheckState(this IEnumerable<CheckBox> boxes)
        {
            var lst = boxes as List<CheckBox> ?? boxes.ToList();
            if (lst.All(chk => chk.Checked)) return CheckState.Checked;
            return lst.All(chk => !chk.Checked) ? CheckState.Unchecked : CheckState.Indeterminate;
        }

        public static T WithName<T>(this T ctrl, string name) where T : Control
        {
            ctrl.Name = name;
            return ctrl;
        }

        public static void AddRange<T>(this Control.ControlCollection col, IEnumerable<T> items) where T : Control =>
            col.AddRange(items.Cast<Control>().ToArray());

        public static T Tag<T>(this Control ctrl) => (T)ctrl.Tag;
    }
}
