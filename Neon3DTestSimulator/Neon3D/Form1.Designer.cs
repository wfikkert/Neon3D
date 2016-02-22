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
            this.Show3D = new System.Windows.Forms.Button();
            this.Debug = new System.Windows.Forms.RichTextBox();
            this.Coordinates = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Show3D
            // 
            this.Show3D.Location = new System.Drawing.Point(966, 354);
            this.Show3D.Name = "Show3D";
            this.Show3D.Size = new System.Drawing.Size(312, 23);
            this.Show3D.TabIndex = 0;
            this.Show3D.Text = "Show3D";
            this.Show3D.UseVisualStyleBackColor = true;
            this.Show3D.Click += new System.EventHandler(this.Show3D_Click);
            // 
            // Debug
            // 
            this.Debug.Location = new System.Drawing.Point(966, 0);
            this.Debug.Name = "Debug";
            this.Debug.Size = new System.Drawing.Size(312, 275);
            this.Debug.TabIndex = 1;
            this.Debug.Text = "";
            // 
            // Coordinates
            // 
            this.Coordinates.Location = new System.Drawing.Point(966, 328);
            this.Coordinates.Name = "Coordinates";
            this.Coordinates.Size = new System.Drawing.Size(312, 20);
            this.Coordinates.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(963, 312);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Coordinates";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1279, 588);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Coordinates);
            this.Controls.Add(this.Debug);
            this.Controls.Add(this.Show3D);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Show3D;
        private System.Windows.Forms.RichTextBox Debug;
        private System.Windows.Forms.TextBox Coordinates;
        private System.Windows.Forms.Label label1;
    }
}

