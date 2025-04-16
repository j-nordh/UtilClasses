namespace UtilClasses.Winforms
{
    partial class LabelledLinkLabel
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
            this.ulUnit = new System.Windows.Forms.Label();
            this.ulbl = new System.Windows.Forms.Label();
            this.link = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // ulUnit
            // 
            this.ulUnit.AutoSize = true;
            this.ulUnit.Dock = System.Windows.Forms.DockStyle.Right;
            this.ulUnit.Location = new System.Drawing.Point(69, 13);
            this.ulUnit.MinimumSize = new System.Drawing.Size(0, 20);
            this.ulUnit.Name = "ulUnit";
            this.ulUnit.Size = new System.Drawing.Size(0, 20);
            this.ulUnit.TabIndex = 5;
            this.ulUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ulUnit.UseCompatibleTextRendering = true;
            // 
            // ulbl
            // 
            this.ulbl.Dock = System.Windows.Forms.DockStyle.Top;
            this.ulbl.Location = new System.Drawing.Point(0, 0);
            this.ulbl.Name = "ulbl";
            this.ulbl.Size = new System.Drawing.Size(69, 13);
            this.ulbl.TabIndex = 3;
            this.ulbl.Text = "Test";
            // 
            // link
            // 
            this.link.AutoSize = true;
            this.link.Location = new System.Drawing.Point(3, 15);
            this.link.Name = "link";
            this.link.Size = new System.Drawing.Size(63, 13);
            this.link.TabIndex = 6;
            this.link.TabStop = true;
            this.link.Text = "Cepheid AB";
            // 
            // LabelledLinkLabel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.link);
            this.Controls.Add(this.ulUnit);
            this.Controls.Add(this.ulbl);
            this.Name = "LabelledLinkLabel";
            this.Size = new System.Drawing.Size(69, 28);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.Label ulUnit;
        internal System.Windows.Forms.Label ulbl;
        private System.Windows.Forms.LinkLabel link;
    }
}
