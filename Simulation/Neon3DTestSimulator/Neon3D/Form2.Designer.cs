﻿namespace Neon3D
{
    partial class Form2
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
        public void InitializeComponent()
        {
            
            this.Debug = new System.Windows.Forms.RichTextBox();
            
            this.SuspendLayout();
            
            
            
            
            // 
            // Debug
            // 
            this.Debug.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.Debug.Location = new System.Drawing.Point(26, 12);
            this.Debug.Name = "Debug";
            this.Debug.Size = new System.Drawing.Size(312, 275);
            this.Debug.TabIndex = 11;
            this.Debug.Text = "";
            this.Debug.TextChanged += new System.EventHandler(this.Debug_TextChanged);
           
            
            
            
            
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 448);
            this.Controls.Add(this.SaveAs);
            this.Controls.Add(this.OpenFile);
            this.Controls.Add(this.drawLines);
            this.Controls.Add(this.resetNodes);
            this.Controls.Add(this.ResetScr);
            this.Controls.Add(this.Debug);
            this.Name = "Form2";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button resetNodes;
        private System.Windows.Forms.Button ResetScr;
        private System.Windows.Forms.RichTextBox Debug;
        private System.Windows.Forms.Button drawLines;
        private System.Windows.Forms.Button OpenFile;
        private System.Windows.Forms.Button SaveAs;
    }
}