namespace Neon3D
{
    partial class FpgaUpload
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
            this.FPGAProgress = new System.Windows.Forms.ProgressBar();
            this.StartUpload = new System.Windows.Forms.Button();
            this.AbortUpload = new System.Windows.Forms.Button();
            this.FpgaUploadInformation = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // FPGAProgress
            // 
            this.FPGAProgress.Location = new System.Drawing.Point(12, 338);
            this.FPGAProgress.Name = "FPGAProgress";
            this.FPGAProgress.Size = new System.Drawing.Size(737, 23);
            this.FPGAProgress.TabIndex = 0;
            // 
            // StartUpload
            // 
            this.StartUpload.Location = new System.Drawing.Point(268, 372);
            this.StartUpload.Name = "StartUpload";
            this.StartUpload.Size = new System.Drawing.Size(75, 23);
            this.StartUpload.TabIndex = 1;
            this.StartUpload.Text = "Upload";
            this.StartUpload.UseVisualStyleBackColor = true;
            this.StartUpload.Click += new System.EventHandler(this.StartUpload_Click);
            // 
            // AbortUpload
            // 
            this.AbortUpload.Location = new System.Drawing.Point(413, 372);
            this.AbortUpload.Name = "AbortUpload";
            this.AbortUpload.Size = new System.Drawing.Size(75, 23);
            this.AbortUpload.TabIndex = 2;
            this.AbortUpload.Text = "Abort";
            this.AbortUpload.UseVisualStyleBackColor = true;
            this.AbortUpload.Click += new System.EventHandler(this.AbortUpload_Click);
            // 
            // FpgaUploadInformation
            // 
            this.FpgaUploadInformation.Location = new System.Drawing.Point(13, 12);
            this.FpgaUploadInformation.Name = "FpgaUploadInformation";
            this.FpgaUploadInformation.Size = new System.Drawing.Size(736, 305);
            this.FpgaUploadInformation.TabIndex = 3;
            this.FpgaUploadInformation.Text = "";
            // 
            // FpgaUpload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(761, 407);
            this.Controls.Add(this.FpgaUploadInformation);
            this.Controls.Add(this.AbortUpload);
            this.Controls.Add(this.StartUpload);
            this.Controls.Add(this.FPGAProgress);
            this.Name = "FpgaUpload";
            this.Text = "FpgaUpload";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar FPGAProgress;
        private System.Windows.Forms.Button StartUpload;
        private System.Windows.Forms.Button AbortUpload;
        private System.Windows.Forms.RichTextBox FpgaUploadInformation;
    }
}