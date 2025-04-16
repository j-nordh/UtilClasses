using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilClasses.Winforms.Extensions;

namespace UtilClasses.Winforms
{
    
    public class ExtendedTabCtrl : System.Windows.Forms.TabControl
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int TCM_SETMINTABWIDTH = 0x1300 + 49;

        public event EventHandler<MouseEventArgs> NewButtonClicked;
        private TabPage AddPageDummy;
        
        //private bool _visible = true;
        public ExtendedTabCtrl()
        {
            float dpiX = 0;
            Graphics graphics = CreateGraphics();
            dpiX = graphics.DpiX;
            float fFactor = dpiX / 96;
            
            HandleCreated += (s, e) => SendMessage(Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr) (int)(fFactor*32));
            DrawItem += ExtendedTabCtrl_DrawItem;
            MouseDown += ExtendedTabCtrl_MouseDown;
            Selecting += ExtendedTabCtrl_Selecting;
            DrawMode = TabDrawMode.OwnerDrawFixed;
            ControlAdded += (sender, args) =>
            {
                Refresh();
            };
            ControlRemoved += (sender, args) =>Refresh();
            AddPageDummy = new TabPage();
            TabPages.Add(AddPageDummy); //the "add page" dummy page
        }

        private void ExtendedTabCtrl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            //Don't select "AddNew"
            if (TabPages.Contains(AddPageDummy))
            {
                if (e.TabPageIndex == TabCount - 1)
                    e.Cancel = true;
            }
        }

        private void ExtendedTabCtrl_MouseDown(object sender, MouseEventArgs e)
        {
            //"AddNew" needs to exist to be clicked
            if (TabPages.Contains(AddPageDummy))
            {
                var lastIndex = TabCount - 1;
                if (!GetTabRect(lastIndex).Contains(e.Location)) return;
                if (!TabPages[TabCount - 1].Enabled) return;
                NewButtonClicked?.Invoke(this, e);
            }
        }

        private void ExtendedTabCtrl_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= TabCount)
                return;
            var tabRect = GetTabRect(e.Index);
            var tabPage = TabPages[e.Index];
            if (e.Index == TabCount - 1 && Last == AddPageDummy)
            {
                //if (!_visible) return;

                float dpiX = 0;
                Graphics graphics = CreateGraphics();
                dpiX = graphics.DpiX;
                float fFactor = dpiX / 96;

                tabRect.Inflate(-2, -2);
                var addImage = Properties.Resources.plus;
                var cm = new ColorMatrix();
                cm.Matrix33 = tabPage.Enabled ? 0.65f : 0.5f;
                var attr = new ImageAttributes();
                attr.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                var rect = new Rectangle((int) (tabRect.Left + (tabRect.Width - addImage.Width*fFactor)/2),
                    (int) (tabRect.Top + (tabRect.Height - addImage.Height*fFactor)/2), (int) (addImage.Width * fFactor), (int) (addImage.Height*fFactor));
                e.Graphics.DrawImage(addImage,rect,0,0, addImage.Width, addImage.Height, GraphicsUnit.Pixel, attr);                
            }
            else
            {
                var foreColor = tabPage.ForeColor;
                if (e.Index == SelectedIndex)
                {
                    var rect = tabRect;// new Rectangle(tabRect.Left+1,tabRect.Y, tabRect.Width-2, tabRect.Height-1);
                    var brush = new SolidBrush(ControlPaint.Light(tabPage.BackColor));
                    e.Graphics.FillRectangle(brush, rect);
                }
                else
                {
                    foreColor = foreColor.Brighten(55);
                    //foreColor = ControlPaint.Light(foreColor);
                    //foreColor = ControlPaint.LightLight(foreColor);
                }
                TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font,
                    tabRect, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private TabPage Last => TabPages[TabCount - 1];

        public bool AddNewVisible
        {
            //"AddNew" needs to exist to be visible
            get { return TabPages.Contains(AddPageDummy); }
            set
            {
                //if (_visible == value) return;
                //_visible = value;
                if (value && !TabPages.Contains(AddPageDummy))
                {
                    TabPages.Add(AddPageDummy);
                    AddPageDummy.Show();
                }
                if (!value && TabPages.Contains(AddPageDummy))
                {
                    AddPageDummy.Hide();
                    TabPages.Remove(AddPageDummy);
                }
                Refresh();
            }
        }
        public bool AddNewEnabled
        {
            //"AddNew" needs to exist to be enabled
            get { return TabPages.Contains(AddPageDummy) && Last.Enabled; }
            set
            {
                if (!TabPages.Contains(AddPageDummy) || Last.Enabled == value) return;
                Last.Enabled = value;
                Refresh();
            }
        }

        //Pick all TabPages but "AddNew" if that exist
        public IEnumerable<TabPage> ContentPages => TabPages.Contains(AddPageDummy) ? TabPages.Cast<TabPage>().Take(TabCount - 1) : TabPages.Cast<TabPage>().Take(TabCount);

    }
}
