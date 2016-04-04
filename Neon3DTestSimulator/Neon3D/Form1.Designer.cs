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
        /// 

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label Front;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.Front = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.zoomTL = new System.Windows.Forms.Label();
            this.zoomBL = new System.Windows.Forms.Label();
            this.zoomBR = new System.Windows.Forms.Label();
            this.zoomTR = new System.Windows.Forms.Label();
            this.RotationTL = new System.Windows.Forms.Label();
            this.RotationBL = new System.Windows.Forms.Label();
            this.RotationTR = new System.Windows.Forms.Label();
            this.RotationBR = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Top";
            // 
            // Front
            // 
            this.Front.AutoSize = true;
            this.Front.Location = new System.Drawing.Point(13, 553);
            this.Front.Name = "Front";
            this.Front.Size = new System.Drawing.Size(31, 13);
            this.Front.TabIndex = 1;
            this.Front.Text = "Front";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(973, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Right";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(973, 553);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(21, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "3D";
            // 
            // zoomTL
            // 
            this.zoomTL.AutoSize = true;
            this.zoomTL.Location = new System.Drawing.Point(13, 26);
            this.zoomTL.Name = "zoomTL";
            this.zoomTL.Size = new System.Drawing.Size(51, 13);
            this.zoomTL.TabIndex = 4;
            this.zoomTL.Text = "Zoom: 1x";
            // 
            // zoomBL
            // 
            this.zoomBL.AutoSize = true;
            this.zoomBL.Location = new System.Drawing.Point(13, 568);
            this.zoomBL.Name = "zoomBL";
            this.zoomBL.Size = new System.Drawing.Size(51, 13);
            this.zoomBL.TabIndex = 5;
            this.zoomBL.Text = "Zoom: 1x";
            // 
            // zoomBR
            // 
            this.zoomBR.AutoSize = true;
            this.zoomBR.Location = new System.Drawing.Point(973, 566);
            this.zoomBR.Name = "zoomBR";
            this.zoomBR.Size = new System.Drawing.Size(51, 13);
            this.zoomBR.TabIndex = 6;
            this.zoomBR.Text = "Zoom: 1x";
            // 
            // zoomTR
            // 
            this.zoomTR.AutoSize = true;
            this.zoomTR.Location = new System.Drawing.Point(973, 26);
            this.zoomTR.Name = "zoomTR";
            this.zoomTR.Size = new System.Drawing.Size(51, 13);
            this.zoomTR.TabIndex = 7;
            this.zoomTR.Text = "Zoom: 1x";
            // 
            // RotationTL
            // 
            this.RotationTL.AutoSize = true;
            this.RotationTL.Location = new System.Drawing.Point(13, 39);
            this.RotationTL.Name = "RotationTL";
            this.RotationTL.Size = new System.Drawing.Size(59, 13);
            this.RotationTL.TabIndex = 8;
            this.RotationTL.Text = "Rotation: 0";
            // 
            // RotationBL
            // 
            this.RotationBL.AutoSize = true;
            this.RotationBL.Location = new System.Drawing.Point(13, 581);
            this.RotationBL.Name = "RotationBL";
            this.RotationBL.Size = new System.Drawing.Size(59, 13);
            this.RotationBL.TabIndex = 9;
            this.RotationBL.Text = "Rotation: 0";
            // 
            // RotationTR
            // 
            this.RotationTR.AutoSize = true;
            this.RotationTR.Location = new System.Drawing.Point(973, 39);
            this.RotationTR.Name = "RotationTR";
            this.RotationTR.Size = new System.Drawing.Size(59, 13);
            this.RotationTR.TabIndex = 10;
            this.RotationTR.Text = "Rotation: 0";
            // 
            // RotationBR
            // 
            this.RotationBR.AutoSize = true;
            this.RotationBR.Location = new System.Drawing.Point(973, 579);
            this.RotationBR.Name = "RotationBR";
            this.RotationBR.Size = new System.Drawing.Size(59, 13);
            this.RotationBR.TabIndex = 11;
            this.RotationBR.Text = "Rotation: 0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1279, 588);
            this.Controls.Add(this.RotationBR);
            this.Controls.Add(this.RotationTR);
            this.Controls.Add(this.RotationBL);
            this.Controls.Add(this.RotationTL);
            this.Controls.Add(this.zoomTR);
            this.Controls.Add(this.zoomBR);
            this.Controls.Add(this.zoomBL);
            this.Controls.Add(this.zoomTL);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Front);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.DoubleClick += new System.EventHandler(this.Form1_DoubleClick);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseClick);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseWheel);
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private System.Windows.Forms.Label zoomTL;
        private System.Windows.Forms.Label zoomBL;
        private System.Windows.Forms.Label zoomBR;
        private System.Windows.Forms.Label zoomTR;
        private System.Windows.Forms.Label RotationTL;
        private System.Windows.Forms.Label RotationBL;
        private System.Windows.Forms.Label RotationTR;
        private System.Windows.Forms.Label RotationBR;
    }
}

