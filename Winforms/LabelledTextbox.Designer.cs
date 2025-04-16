using System.Drawing;
using System.Windows.Forms;

namespace UtilClasses.Winforms
{

    using Microsoft.VisualBasic;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    partial class LabelledTextbox : System.Windows.Forms.UserControl
    {

        //UserControl overrides dispose to clean up the component list.
        [System.Diagnostics.DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        //Required by the Windows Form Designer

        private System.ComponentModel.IContainer components;
        //NOTE: The following procedure is required by the Windows Form Designer
        //It can be modified using the Windows Form Designer.  
        //Do not modify it using the code editor.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.ulbl = new System.Windows.Forms.Label();
            this.ulUnit = new System.Windows.Forms.Label();
            this.utxt = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ulbl
            // 
            this.ulbl.Dock = System.Windows.Forms.DockStyle.Top;
            this.ulbl.Location = new System.Drawing.Point(0, 0);
            this.ulbl.Name = "ulbl";
            this.ulbl.Size = new System.Drawing.Size(150, 13);
            this.ulbl.TabIndex = 0;
            this.ulbl.Text = "Test";
            // 
            // ulUnit
            // 
            this.ulUnit.AutoSize = true;
            this.ulUnit.Dock = System.Windows.Forms.DockStyle.Right;
            this.ulUnit.Location = new System.Drawing.Point(150, 13);
            this.ulUnit.MinimumSize = new System.Drawing.Size(0, 20);
            this.ulUnit.Name = "ulUnit";
            this.ulUnit.Size = new System.Drawing.Size(0, 20);
            this.ulUnit.TabIndex = 2;
            this.ulUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ulUnit.UseCompatibleTextRendering = true;
            // 
            // utxt
            // 
            this.utxt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.utxt.Location = new System.Drawing.Point(0, 13);
            this.utxt.Name = "utxt";
            this.utxt.Size = new System.Drawing.Size(150, 20);
            this.utxt.TabIndex = 1;
            // 
            // LabelledTextbox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.utxt);
            this.Controls.Add(this.ulUnit);
            this.Controls.Add(this.ulbl);
            this.Name = "LabelledTextbox";
            this.Size = new System.Drawing.Size(150, 32);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        internal Label ulbl;
        internal Label ulUnit;
        internal TextBox utxt;
    }
}
