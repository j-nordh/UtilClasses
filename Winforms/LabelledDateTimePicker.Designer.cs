namespace UtilClasses.Winforms
{
    partial class LabelledDateTimePicker
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
            this.picker = new System.Windows.Forms.DateTimePicker();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ulbl
            // 
            this.ulbl.Dock = System.Windows.Forms.DockStyle.Top;
            this.ulbl.Location = new System.Drawing.Point(0, 0);
            this.ulbl.Name = "ulbl";
            this.ulbl.Size = new System.Drawing.Size(130, 13);
            this.ulbl.TabIndex = 3;
            this.ulbl.Text = "Test";
            this.ulbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // picker
            // 
            this.picker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.picker.Location = new System.Drawing.Point(0, 14);
            this.picker.Name = "picker";
            this.picker.Size = new System.Drawing.Size(93, 20);
            this.picker.TabIndex = 4;
            // 
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.Location = new System.Drawing.Point(99, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(31, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "Nu";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // LabelledDateTimePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.picker);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ulbl);
            this.Name = "LabelledDateTimePicker";
            this.Size = new System.Drawing.Size(130, 37);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.Label ulbl;
        private System.Windows.Forms.DateTimePicker picker;
        private System.Windows.Forms.Button button1;
    }
}
