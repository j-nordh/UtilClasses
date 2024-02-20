using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UtilClasses.Winforms
{
    public partial class InputForm : Form
    {
        private readonly MessageBoxButtons _buttons;
        private readonly string _question;
        private readonly string _caption;
        private readonly Control _ctrl;
        private readonly List<IInputValidator> _validators;

       public InputForm(MessageBoxButtons buttons, string question, string caption, IEnumerable<object> values = null)
        {
            _buttons = buttons;
            _question = question;
            _caption = caption;
            InitializeComponent();
            var vals = values as object[] ?? values?.ToArray();
            if (vals == null || !vals.Any())
            {
                _ctrl = txtAnswer;
                cmboValue.Visible = false;
            }
            else
            {
                _ctrl = cmboValue;
                txtAnswer.Visible = false;
                cmboValue.DataSource = values;
            }
            _validators = new List<IInputValidator>();
        }


        private void btnOk_Click(object sender, EventArgs e)
        {
            var failingValidator = _validators.FirstOrDefault(v => !v.Validate(Value));
            if (null != failingValidator)
            {
                MessageBox.Show(failingValidator.ErrorMessage, "", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            DialogResult = _buttons ==MessageBoxButtons.YesNo ? DialogResult.Yes : DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = _buttons == MessageBoxButtons.YesNo ? DialogResult.No : DialogResult.Cancel;
            Close();
        }

        private void Inputform_Load(object sender, EventArgs e)
        {
            //ZOOM
            float dpiX = 0;
            Graphics graphics = CreateGraphics();
            dpiX = graphics.DpiX;
            float fFactor = dpiX / 96;

            btnOk.Height = (int)(btnOk.Height * fFactor);
            btnCancel.Height = (int)(btnCancel.Height * fFactor);

            btnOk.Width = (int)(btnOk.Width * fFactor);
            btnCancel.Width = (int)(btnCancel.Width * fFactor);

            txtAnswer.Width = (int) (txtAnswer.Width*fFactor);
            lblQuestion.MaximumSize = new Size((int) (lblQuestion.MaximumSize.Width*fFactor), (int) (lblQuestion.MaximumSize.Height * fFactor));

            lblQuestion.Padding = new Padding((int)(lblQuestion.Padding.Left * fFactor), 
                (int)(lblQuestion.Padding.Top * fFactor), 
                (int)(lblQuestion.Padding.Right * fFactor), 
                (int) (lblQuestion.Padding.Bottom*fFactor));

            tableLayoutPanel2.Padding = new Padding((int) (tableLayoutPanel2.Padding.Left*fFactor), 
                (int) (tableLayoutPanel2.Padding.Top*fFactor), 
                (int) (tableLayoutPanel2.Padding.Right*fFactor), 
                (int) (tableLayoutPanel2.Padding.Bottom*fFactor));
            
            lblQuestion.Text = _question;
            
            switch (_buttons)
            {
                case MessageBoxButtons.OK:
                    btnCancel.Visible = false;
                    btnOk.Text = "Ok";// _resolver.GetByKey("ok");
                    break;
                case MessageBoxButtons.YesNo:
                    btnCancel.Text = "Nej";// _resolver.GetByKey("no");
                    btnOk.Text = "Ja"; //_resolver.GetByKey("yes");
                    break;
                case MessageBoxButtons.OKCancel:
                    btnCancel.Text = "Avbryt";// _resolver.GetByKey("cancel");
                    btnOk.Text = "Ok";// _resolver.GetByKey("ok");
                    break;
                default:
                    throw new NotSupportedException();
            }
            Text = _caption;
            ClientSize = new Size(flowLayoutPanel1.Width + 2*flowLayoutPanel1.Left, flowLayoutPanel1.Height + 2*flowLayoutPanel1.Top);
            CenterToParent();
            _ctrl.Select();
        }

        public object Value
        {
            get
            {
                if (_ctrl ==txtAnswer)
                {
                    return txtAnswer.Text;
                }
                return cmboValue.SelectedItem;


            }
            set
            {
                if(_ctrl == txtAnswer)
                    txtAnswer.Text = value.ToString();
                cmboValue.SelectedItem = value;
            }
        }

        public string DisplayMember
        {
            get { return cmboValue.DisplayMember; }
            set { cmboValue.DisplayMember = value; }
        }

        public string ValueMember
        {
            get { return cmboValue.ValueMember; }
            set { cmboValue.ValueMember = value; }
        }

        public void AddValidator(IInputValidator validator)
        {
            _validators.Add(validator);
        }

        public void SetTextAnswerMaxLength(int maxLength)
        {
            txtAnswer.MaxLength = maxLength;
        }
    }
}
