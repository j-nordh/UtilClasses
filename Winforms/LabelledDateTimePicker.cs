using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilClasses.Winforms
{
    public partial class LabelledDateTimePicker : UserControl
    {
        public event EventHandler ValueChanged;
        public LabelledDateTimePicker()
        {
            InitializeComponent();
            picker.ValueChanged +=(_,e)=>ValueChanged?.Invoke(this, e);
        }

        public DateTime Value { get => picker.Value; set => picker.Value = value; }
        public string Caption { get => ulbl.Text; set => ulbl.Text = value; }
    }
}
