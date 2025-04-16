namespace UtilClasses.Winforms
{
    partial class TagList
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lst = new System.Windows.Forms.ListBox();
            this.ctxt = new UtilClasses.Winforms.CueTextBox();
            this.SuspendLayout();
            // 
            // lst
            // 
            this.lst.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lst.FormattingEnabled = true;
            this.lst.Location = new System.Drawing.Point(0, 20);
            this.lst.Name = "lst";
            this.lst.Size = new System.Drawing.Size(150, 130);
            this.lst.TabIndex = 1;
            this.lst.SelectedIndexChanged += new System.EventHandler(this.lst_SelectedIndexChanged);
            this.lst.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lst_KeyDown);
            // 
            // ctxt
            // 
            this.ctxt.Cue = null;
            this.ctxt.Dock = System.Windows.Forms.DockStyle.Top;
            this.ctxt.Location = new System.Drawing.Point(0, 0);
            this.ctxt.Name = "ctxt";
            this.ctxt.Size = new System.Drawing.Size(150, 20);
            this.ctxt.TabIndex = 0;
            this.ctxt.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ctxt_KeyDown);
            // 
            // TagList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lst);
            this.Controls.Add(this.ctxt);
            this.Name = "TagList";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CueTextBox ctxt;
        private System.Windows.Forms.ListBox lst;
    }
}
