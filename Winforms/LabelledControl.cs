using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UtilClasses.Winforms
{
    public class LabelledControl<T> : LabelledControl<T, EventArgs> where T:Control, new()
    { }
    public class LabelledControl<T, TChangedArg> : UserControl where T : Control, new()
    {
        protected Label _lblCaption;
        public T Ctrl { get; }
        public event EventHandler<TChangedArg> Changed;

        
        public LabelledControl()
        {
            Ctrl = new T
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 13),
                Name = "lblContent",
                Size = new Size(34, 16),
                TabIndex = 1,
                Text = "Text"
            };
            ResetCaption();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ResetCaption();

            
            Controls.Add(Ctrl);
            Controls.Add(_lblCaption);
            CaptionDockStyle = DockStyle.Top;
        }
        public Orientation CaptionOrientation { get; set; }
        private void ResetCaption()
        {
            var c = "Caption";
            if (null != _lblCaption)
            {
                Controls.Remove(_lblCaption);
                c = _lblCaption.Text;
            }
            _lblCaption = CaptionOrientation == Orientation.Vertical 
                ? new VerticalLabel() 
                : new Label();

            _lblCaption.Dock = DockStyle.Top;
            _lblCaption.Location = new Point(0, 0);
            _lblCaption.Name = "lblCaption";
            _lblCaption.Size = new Size(43, 13);
            _lblCaption.TabIndex = 0;
            _lblCaption.Text = "Caption";
            Controls.Add(_lblCaption);
        }

        protected void RaiseChanged(TChangedArg arg)
        {
            Changed?.Invoke(this, arg);
        }

        protected void RaiseChanged(object s, TChangedArg arg)
        {
            Changed?.Invoke(this, arg);
        }

        public DockStyle CaptionDockStyle
        {
            get { return _lblCaption?.Dock ?? DockStyle.None; }
            set
            {
                _lblCaption.Dock = value;
                if (value == DockStyle.Left || value == DockStyle.Right)
                    _lblCaption.TextAlign = ContentAlignment.MiddleCenter;
                else
                    _lblCaption.TextAlign = ContentAlignment.TopLeft;
            }
        }
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override string Text
        {
            get { return Ctrl?.Text; }
            set { if(null!=Ctrl) Ctrl.Text = value; }
        }

        public string Caption
        {
            get => _lblCaption?.Text ?? "";
            set
            {
                if (_lblCaption == null) return;
                _lblCaption.Text = value;
                _lblCaption.Width = TextRenderer.MeasureText(value, Ctrl.Font).Width;
            }
        }
    }
}
