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
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 16);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Top";
            // 
            // Front
            // 
            this.Front.AutoSize = true;
            this.Front.Location = new System.Drawing.Point(17, 681);
            this.Front.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Front.Name = "Front";
            this.Front.Size = new System.Drawing.Size(38, 16);
            this.Front.TabIndex = 1;
            this.Front.Text = "Front";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1297, 16);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Right";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1297, 681);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 16);
            this.label3.TabIndex = 3;
            this.label3.Text = "3D";
            // 
            // zoomTL
            // 
            this.zoomTL.AutoSize = true;
            this.zoomTL.Location = new System.Drawing.Point(17, 44);
            this.zoomTL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.zoomTL.Name = "zoomTL";
            this.zoomTL.Size = new System.Drawing.Size(62, 16);
            this.zoomTL.TabIndex = 4;
            this.zoomTL.Text = "Zoom: 1x";
            // 
            // zoomBL
            // 
            this.zoomBL.AutoSize = true;
            this.zoomBL.Location = new System.Drawing.Point(17, 699);
            this.zoomBL.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.zoomBL.Name = "zoomBL";
            this.zoomBL.Size = new System.Drawing.Size(62, 16);
            this.zoomBL.TabIndex = 5;
            this.zoomBL.Text = "Zoom: 1x";
            // 
            // zoomBR
            // 
            this.zoomBR.AutoSize = true;
            this.zoomBR.Location = new System.Drawing.Point(1297, 697);
            this.zoomBR.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.zoomBR.Name = "zoomBR";
            this.zoomBR.Size = new System.Drawing.Size(62, 16);
            this.zoomBR.TabIndex = 6;
            this.zoomBR.Text = "Zoom: 1x";
            // 
            // zoomTR
            // 
            this.zoomTR.AutoSize = true;
            this.zoomTR.Location = new System.Drawing.Point(1297, 32);
            this.zoomTR.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.zoomTR.Name = "zoomTR";
            this.zoomTR.Size = new System.Drawing.Size(62, 16);
            this.zoomTR.TabIndex = 7;
            this.zoomTR.Text = "Zoom: 1x";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1705, 724);
            this.Controls.Add(this.zoomTR);
            this.Controls.Add(this.zoomBR);
            this.Controls.Add(this.zoomBL);
            this.Controls.Add(this.zoomTL);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Front);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.DoubleClick += new System.EventHandler(this.Form1_DoubleClick);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
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
    }
}

