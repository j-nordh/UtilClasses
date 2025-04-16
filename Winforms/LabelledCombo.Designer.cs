namespace UtilClasses.Winforms
{
    partial class LabelledCombo
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
            this.ulbl = new System.Windows.Forms.Label();
            this.cmbo = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // ulbl
            // 
            this.ulbl.Dock = System.Windows.Forms.DockStyle.Top;
            this.ulbl.Location = new System.Drawing.Point(0, 0);
            this.ulbl.Name = "ulbl";
            this.ulbl.Size = new System.Drawing.Size(140, 13);
            this.ulbl.TabIndex = 1;
            this.ulbl.Text = "Test";
            this.ulbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmbo
            // 
            this.cmbo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbo.FormattingEnabled = true;
            this.cmbo.Location = new System.Drawing.Point(0, 13);
            this.cmbo.Name = "cmbo";
            this.cmbo.Size = new System.Drawing.Size(140, 21);
            this.cmbo.TabIndex = 2;
            this.cmbo.SelectedIndexChanged += new System.EventHandler(this.cmbo_SelectedIndexChanged);
            // 
            // LabelledCombo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmbo);
            this.Controls.Add(this.ulbl);
            this.Name = "LabelledCombo";
            this.Size = new System.Drawing.Size(140, 34);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Label ulbl;
        private System.Windows.Forms.ComboBox cmbo;
    }
}
