using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilClasses.Winforms
{
    public partial class OkCancelForm : Form
    {
        public OkCancelForm()
        {
            InitializeComponent();
        }
        protected virtual void OnOk() { }
        protected virtual void OnCancel() { }
        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            OnOk();
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            OnCancel();
            Close();
        }
    }
}
