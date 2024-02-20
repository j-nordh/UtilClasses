namespace UtilClasses.Winforms
{
    partial class BasicManager
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
            this.split = new System.Windows.Forms.SplitContainer();
            this.lstvItems = new System.Windows.Forms.ListView();
            this.pnlEditor = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.tsbReload = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.split)).BeginInit();
            this.split.Panel1.SuspendLayout();
            this.split.Panel2.SuspendLayout();
            this.split.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // split
            // 
            this.split.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split.Location = new System.Drawing.Point(0, 0);
            this.split.Name = "split";
            this.split.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // split.Panel1
            // 
            this.split.Panel1.Controls.Add(this.lstvItems);
            // 
            // split.Panel2
            // 
            this.split.Panel2.Controls.Add(this.pnlEditor);
            this.split.Panel2.Controls.Add(this.toolStrip1);
            this.split.Size = new System.Drawing.Size(695, 547);
            this.split.SplitterDistance = 231;
            this.split.TabIndex = 0;
            // 
            // lstvItems
            // 
            this.lstvItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstvItems.Location = new System.Drawing.Point(0, 0);
            this.lstvItems.Name = "lstvItems";
            this.lstvItems.Size = new System.Drawing.Size(695, 231);
            this.lstvItems.TabIndex = 0;
            this.lstvItems.UseCompatibleStateImageBehavior = false;
            this.lstvItems.View = System.Windows.Forms.View.Details;
            this.lstvItems.SelectedIndexChanged += new System.EventHandler(this.LstvItems_SelectedIndexChanged);
            // 
            // pnlEditor
            // 
            this.pnlEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEditor.Location = new System.Drawing.Point(0, 27);
            this.pnlEditor.Name = "pnlEditor";
            this.pnlEditor.Size = new System.Drawing.Size(695, 285);
            this.pnlEditor.TabIndex = 4;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbSave,
            this.tsbReload});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(695, 27);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbSave
            // 
            this.tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSave.Image = global::UtilClasses.Winforms.Properties.Resources.Save;
            this.tsbSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSave.Name = "tsbSave";
            this.tsbSave.Size = new System.Drawing.Size(24, 24);
            this.tsbSave.Text = "Spara";
            this.tsbSave.Click += new System.EventHandler(this.tsbSave_Click);
            // 
            // tsbReload
            // 
            this.tsbReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbReload.Image = global::UtilClasses.Winforms.Properties.Resources.Reload;
            this.tsbReload.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbReload.Name = "tsbReload";
            this.tsbReload.Size = new System.Drawing.Size(24, 24);
            this.tsbReload.Text = "Läs om";
            this.tsbReload.Click += new System.EventHandler(this.tsbReload_Click);
            // 
            // BasicManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.split);
            this.Name = "BasicManager";
            this.Size = new System.Drawing.Size(695, 547);
            this.Load += new System.EventHandler(this.BasicManager_Load);
            this.split.Panel1.ResumeLayout(false);
            this.split.Panel2.ResumeLayout(false);
            this.split.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split)).EndInit();
            this.split.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer split;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbSave;
        protected System.Windows.Forms.Panel pnlEditor;
        private System.Windows.Forms.ToolStripButton tsbReload;
        protected System.Windows.Forms.ListView lstvItems;
    }
}
