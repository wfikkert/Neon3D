namespace Neon3D
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
            this.resetNodes = new System.Windows.Forms.Button();
            this.ResetScr = new System.Windows.Forms.Button();
            this.Debug = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // resetNodes
            // 
            this.resetNodes.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.resetNodes.Location = new System.Drawing.Point(26, 322);
            this.resetNodes.Name = "resetNodes";
            this.resetNodes.Size = new System.Drawing.Size(310, 23);
            this.resetNodes.TabIndex = 13;
            this.resetNodes.Text = "Reset Nodes";
            this.resetNodes.UseVisualStyleBackColor = true;
            this.resetNodes.Click += new System.EventHandler(this.resetNodes_Click);
            // 
            // ResetScr
            // 
            this.ResetScr.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.ResetScr.Location = new System.Drawing.Point(26, 293);
            this.ResetScr.Name = "ResetScr";
            this.ResetScr.Size = new System.Drawing.Size(312, 23);
            this.ResetScr.TabIndex = 12;
            this.ResetScr.Text = "Reset Screen";
            this.ResetScr.UseVisualStyleBackColor = true;
            this.ResetScr.Click += new System.EventHandler(this.ResetScr_Click);
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
    }
}