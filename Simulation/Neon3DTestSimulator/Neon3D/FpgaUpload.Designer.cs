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
            this.radioAllObjects = new System.Windows.Forms.RadioButton();
            this.radioObject1 = new System.Windows.Forms.RadioButton();
            this.radioObject2 = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // FPGAProgress
            // 
            this.FPGAProgress.Location = new System.Drawing.Point(9, 275);
            this.FPGAProgress.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.FPGAProgress.Name = "FPGAProgress";
            this.FPGAProgress.Size = new System.Drawing.Size(553, 19);
            this.FPGAProgress.TabIndex = 0;
            // 
            // StartUpload
            // 
            this.StartUpload.Location = new System.Drawing.Point(201, 302);
            this.StartUpload.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.StartUpload.Name = "StartUpload";
            this.StartUpload.Size = new System.Drawing.Size(56, 19);
            this.StartUpload.TabIndex = 1;
            this.StartUpload.Text = "Upload";
            this.StartUpload.UseVisualStyleBackColor = true;
            this.StartUpload.Click += new System.EventHandler(this.StartUpload_Click);
            // 
            // AbortUpload
            // 
            this.AbortUpload.Location = new System.Drawing.Point(310, 302);
            this.AbortUpload.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.AbortUpload.Name = "AbortUpload";
            this.AbortUpload.Size = new System.Drawing.Size(56, 19);
            this.AbortUpload.TabIndex = 2;
            this.AbortUpload.Text = "Abort";
            this.AbortUpload.UseVisualStyleBackColor = true;
            this.AbortUpload.Click += new System.EventHandler(this.AbortUpload_Click);
            // 
            // FpgaUploadInformation
            // 
            this.FpgaUploadInformation.Location = new System.Drawing.Point(10, 10);
            this.FpgaUploadInformation.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.FpgaUploadInformation.Name = "FpgaUploadInformation";
            this.FpgaUploadInformation.Size = new System.Drawing.Size(553, 249);
            this.FpgaUploadInformation.TabIndex = 3;
            this.FpgaUploadInformation.Text = "";
            // 
            // radioAllObjects
            // 
            this.radioAllObjects.AutoSize = true;
            this.radioAllObjects.Location = new System.Drawing.Point(63, 365);
            this.radioAllObjects.Name = "radioAllObjects";
            this.radioAllObjects.Size = new System.Drawing.Size(75, 17);
            this.radioAllObjects.TabIndex = 4;
            this.radioAllObjects.TabStop = true;
            this.radioAllObjects.Text = "All Objects";
            this.radioAllObjects.UseVisualStyleBackColor = true;
            // 
            // radioObject1
            // 
            this.radioObject1.AutoSize = true;
            this.radioObject1.Location = new System.Drawing.Point(251, 365);
            this.radioObject1.Name = "radioObject1";
            this.radioObject1.Size = new System.Drawing.Size(65, 17);
            this.radioObject1.TabIndex = 5;
            this.radioObject1.TabStop = true;
            this.radioObject1.Text = "Object 1";
            this.radioObject1.UseVisualStyleBackColor = true;
            // 
            // radioObject2
            // 
            this.radioObject2.AutoSize = true;
            this.radioObject2.Location = new System.Drawing.Point(415, 365);
            this.radioObject2.Name = "radioObject2";
            this.radioObject2.Size = new System.Drawing.Size(65, 17);
            this.radioObject2.TabIndex = 6;
            this.radioObject2.TabStop = true;
            this.radioObject2.Text = "Object 2";
            this.radioObject2.UseVisualStyleBackColor = true;
            // 
            // FpgaUpload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(571, 403);
            this.Controls.Add(this.radioObject2);
            this.Controls.Add(this.radioObject1);
            this.Controls.Add(this.radioAllObjects);
            this.Controls.Add(this.FpgaUploadInformation);
            this.Controls.Add(this.AbortUpload);
            this.Controls.Add(this.StartUpload);
            this.Controls.Add(this.FPGAProgress);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FpgaUpload";
            this.Text = "FpgaUpload";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar FPGAProgress;
        private System.Windows.Forms.Button StartUpload;
        private System.Windows.Forms.Button AbortUpload;
        private System.Windows.Forms.RichTextBox FpgaUploadInformation;
        private System.Windows.Forms.RadioButton radioAllObjects;
        private System.Windows.Forms.RadioButton radioObject1;
        private System.Windows.Forms.RadioButton radioObject2;
    }
}