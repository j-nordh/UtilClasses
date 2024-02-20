using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Winforms
{
    public class LabelledNumericTextbox : LabelledControl<NumericTextbox>
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Ctrl.Changed += RaiseChanged;
        }
    }
}
