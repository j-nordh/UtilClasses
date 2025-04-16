using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace UtilClasses.Winforms
{
    public partial class NumericTextbox : UserControl
    {
        private decimal? _val;
        private bool _onlyIntegers;

        public decimal? Value
        {
            get { return _val; }
            set
            {
                
                decimal? newVal = null;
                if (value.HasValue)
                {
                    newVal = Math.Max(Math.Min(Max, value.Value), Min);
                    if (_onlyIntegers) newVal = Math.Truncate(newVal.Value);
                }
                txt.Text = newVal?.ToString(CultureInfo.CurrentCulture) ?? "";
                if (_val == newVal) return;

                _val = newVal;
                
                Changed?.Invoke(this, null);
            }
        }

        public event EventHandler Changed;

        public decimal Increment { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }

        public bool OnlyIntegers
        {
            get { return _onlyIntegers; }
            set
            {
                _onlyIntegers = value;
                if (!_onlyIntegers) return;
                Increment = Math.Truncate(Increment);
                Min = Math.Truncate(Min);
                Max = Math.Truncate(Max);
                if (_val.HasValue) Value = Math.Truncate(_val.Value);
            }
        }
        public string Cue { get => txt.Cue; set => txt.Cue = value; }

        public NumericTextbox()
        {
            InitializeComponent();
            txt.KeyPress += (s, e) =>
            {
                var sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.First();
                e.Handled = !(char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar) || e.KeyChar == sep && !OnlyIntegers && !txt.Text.Contains(sep));
            };
            txt.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Up) Value += Increment;
                if (e.KeyCode == Keys.Down) Value -= Increment;
            };
        }

        public static implicit operator decimal? (NumericTextbox ntb) => ntb.Value;
        public static implicit operator double?(NumericTextbox ntb) => ntb.Value.HasValue? (double) ntb.Value.Value:(double?)null;
        public static explicit operator int?(NumericTextbox ntb) => ntb.Value.HasValue ? (int) ntb.Value:(int?)null;
        public TimeSpan? Seconds => _val==null? (TimeSpan?)null: TimeSpan.FromSeconds((double)_val.Value);
        public TimeSpan? MilliSeconds => _val == null?(TimeSpan?)null: TimeSpan.FromMilliseconds((double)_val.Value);
        public double Percent => (double) _val / 100;
        public int AsInteger => (int) _val;

        private void txt_TextChanged(object sender, EventArgs e)
        {
            decimal val;
            Value = decimal.TryParse(txt.Text, out val) ? val :(decimal?) null;
        }
    }
}
