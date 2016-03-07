namespace Neon3D
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitScrUL = new System.Windows.Forms.ComboBox();
            this.splitScrBL = new System.Windows.Forms.ComboBox();
            this.splitScrUR = new System.Windows.Forms.ComboBox();
            this.splitScrBR = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // splitScrUL
            // 
            this.splitScrUL.FormattingEnabled = true;
            this.splitScrUL.Items.AddRange(new object[] {
            "TOP",
            "BOTTOM",
            "FRONT",
            "BACK",
            "LEFT",
            "RIGHT",
            "3D"});
            this.splitScrUL.Location = new System.Drawing.Point(13, 13);
            this.splitScrUL.Name = "splitScrUL";
            this.splitScrUL.Size = new System.Drawing.Size(187, 21);
            this.splitScrUL.TabIndex = 11;
            this.splitScrUL.SelectedIndexChanged += new System.EventHandler(this.splitScrUL_SelectedIndexChanged);
            // 
            // splitScrBL
            // 
            this.splitScrBL.FormattingEnabled = true;
            this.splitScrBL.Items.AddRange(new object[] {
            "TOP",
            "BOTTOM",
            "FRONT",
            "BACK",
            "LEFT",
            "RIGHT",
            "3D"});
            this.splitScrBL.Location = new System.Drawing.Point(13, 553);
            this.splitScrBL.Name = "splitScrBL";
            this.splitScrBL.Size = new System.Drawing.Size(187, 21);
            this.splitScrBL.TabIndex = 12;
            this.splitScrBL.SelectedIndexChanged += new System.EventHandler(this.splitScrBL_SelectedIndexChanged);
            // 
            // splitScrUR
            // 
            this.splitScrUR.FormattingEnabled = true;
            this.splitScrUR.Items.AddRange(new object[] {
            "TOP",
            "BOTTOM",
            "FRONT",
            "BACK",
            "LEFT",
            "RIGHT",
            "3D"});
            this.splitScrUR.Location = new System.Drawing.Point(973, 13);
            this.splitScrUR.Name = "splitScrUR";
            this.splitScrUR.Size = new System.Drawing.Size(187, 21);
            this.splitScrUR.TabIndex = 13;
            this.splitScrUR.SelectedIndexChanged += new System.EventHandler(this.splitScrUR_SelectedIndexChanged);
            // 
            // splitScrBR
            // 
            this.splitScrBR.FormattingEnabled = true;
            this.splitScrBR.Items.AddRange(new object[] {
            "TOP",
            "BOTTOM",
            "FRONT",
            "BACK",
            "LEFT",
            "RIGHT",
            "3D"});
            this.splitScrBR.Location = new System.Drawing.Point(973, 553);
            this.splitScrBR.Name = "splitScrBR";
            this.splitScrBR.Size = new System.Drawing.Size(187, 21);
            this.splitScrBR.TabIndex = 14;
            this.splitScrBR.SelectedIndexChanged += new System.EventHandler(this.splitScrBR_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1279, 588);
            this.Controls.Add(this.splitScrBR);
            this.Controls.Add(this.splitScrUR);
            this.Controls.Add(this.splitScrBL);
            this.Controls.Add(this.splitScrUL);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.DoubleClick += new System.EventHandler(this.Form1_DoubleClick);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox splitScrUL;
        private System.Windows.Forms.ComboBox splitScrBL;
        private System.Windows.Forms.ComboBox splitScrUR;
        private System.Windows.Forms.ComboBox splitScrBR;
    }
}

