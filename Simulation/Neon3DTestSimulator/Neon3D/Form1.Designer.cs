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
            try
            {
                base.Dispose(disposing);
            }
            catch { }
            
        
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        /// 

        private System.Windows.Forms.Label TopLabel;
        private System.Windows.Forms.Label Front;
        private System.Windows.Forms.Label Right;
        private System.Windows.Forms.Label dried;
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.TopLabel = new System.Windows.Forms.Label();
            this.Front = new System.Windows.Forms.Label();
            this.Right = new System.Windows.Forms.Label();
            this.dried = new System.Windows.Forms.Label();
            this.zoomTL = new System.Windows.Forms.Label();
            this.zoomBL = new System.Windows.Forms.Label();
            this.zoomBR = new System.Windows.Forms.Label();
            this.zoomTR = new System.Windows.Forms.Label();
            this.RotationTL = new System.Windows.Forms.Label();
            this.RotationBL = new System.Windows.Forms.Label();
            this.RotationTR = new System.Windows.Forms.Label();
            this.RotationBR = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.OpenFile = new System.Windows.Forms.ToolStripButton();
            this.SaveAs = new System.Windows.Forms.ToolStripButton();
            this.ResetScr = new System.Windows.Forms.ToolStripButton();
            this.DeleteAllNodes = new System.Windows.Forms.ToolStripButton();
            this.DrawLine = new System.Windows.Forms.ToolStripButton();
            this.GenerateLines = new System.Windows.Forms.ToolStripButton();
            this.DeleteLines = new System.Windows.Forms.ToolStripButton();
            this.OpenDebug = new System.Windows.Forms.ToolStripButton();
            this.UploadFPGA = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TopLabel
            // 
            this.TopLabel.AutoSize = true;
            this.TopLabel.Location = new System.Drawing.Point(0, 0);
            this.TopLabel.Name = "TopLabel";
            this.TopLabel.Size = new System.Drawing.Size(26, 13);
            this.TopLabel.TabIndex = 0;
            this.TopLabel.Text = "Top";
            // 
            // Front
            // 
            this.Front.AutoSize = true;
            this.Front.Location = new System.Drawing.Point(0, 0);
            this.Front.Name = "Front";
            this.Front.Size = new System.Drawing.Size(31, 13);
            this.Front.TabIndex = 1;
            this.Front.Text = "Front";
            // 
            // Right
            // 
            this.Right.AutoSize = true;
            this.Right.Location = new System.Drawing.Point(0, 0);
            this.Right.Name = "Right";
            this.Right.Size = new System.Drawing.Size(32, 13);
            this.Right.TabIndex = 2;
            this.Right.Text = "Right";
            // 
            // dried
            // 
            this.dried.AutoSize = true;
            this.dried.Location = new System.Drawing.Point(0, 0);
            this.dried.Name = "dried";
            this.dried.Size = new System.Drawing.Size(21, 13);
            this.dried.TabIndex = 3;
            this.dried.Text = "3D";
            // 
            // zoomTL
            // 
            this.zoomTL.AutoSize = true;
            this.zoomTL.Location = new System.Drawing.Point(0, 0);
            this.zoomTL.Name = "zoomTL";
            this.zoomTL.Size = new System.Drawing.Size(51, 13);
            this.zoomTL.TabIndex = 4;
            this.zoomTL.Text = "Zoom: 1x";
            // 
            // zoomBL
            // 
            this.zoomBL.AutoSize = true;
            this.zoomBL.Location = new System.Drawing.Point(0, 0);
            this.zoomBL.Name = "zoomBL";
            this.zoomBL.Size = new System.Drawing.Size(51, 13);
            this.zoomBL.TabIndex = 5;
            this.zoomBL.Text = "Zoom: 1x";
            // 
            // zoomBR
            // 
            this.zoomBR.AutoSize = true;
            this.zoomBR.Location = new System.Drawing.Point(0, 0);
            this.zoomBR.Name = "zoomBR";
            this.zoomBR.Size = new System.Drawing.Size(51, 13);
            this.zoomBR.TabIndex = 6;
            this.zoomBR.Text = "Zoom: 1x";
            // 
            // zoomTR
            // 
            this.zoomTR.AutoSize = true;
            this.zoomTR.Location = new System.Drawing.Point(0, 0);
            this.zoomTR.Name = "zoomTR";
            this.zoomTR.Size = new System.Drawing.Size(51, 13);
            this.zoomTR.TabIndex = 7;
            this.zoomTR.Text = "Zoom: 1x";
            // 
            // RotationTL
            // 
            this.RotationTL.Location = new System.Drawing.Point(0, 0);
            this.RotationTL.Name = "RotationTL";
            this.RotationTL.Size = new System.Drawing.Size(100, 23);
            this.RotationTL.TabIndex = 15;
            // 
            // RotationBL
            // 
            this.RotationBL.Location = new System.Drawing.Point(0, 0);
            this.RotationBL.Name = "RotationBL";
            this.RotationBL.Size = new System.Drawing.Size(100, 23);
            this.RotationBL.TabIndex = 14;
            // 
            // RotationTR
            // 
            this.RotationTR.Location = new System.Drawing.Point(0, 0);
            this.RotationTR.Name = "RotationTR";
            this.RotationTR.Size = new System.Drawing.Size(100, 23);
            this.RotationTR.TabIndex = 13;
            // 
            // RotationBR
            // 
            this.RotationBR.AutoSize = true;
            this.RotationBR.Location = new System.Drawing.Point(0, 0);
            this.RotationBR.Name = "RotationBR";
            this.RotationBR.Size = new System.Drawing.Size(59, 13);
            this.RotationBR.TabIndex = 11;
            this.RotationBR.Text = "Rotation: 0";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenFile,
            this.SaveAs,
            this.ResetScr,
            this.DeleteAllNodes,
            this.DrawLine,
            this.GenerateLines,
            this.DeleteLines,
            this.OpenDebug,
            this.UploadFPGA});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1279, 25);
            this.toolStrip1.TabIndex = 12;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // OpenFile
            // 
            this.OpenFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.OpenFile.Image = ((System.Drawing.Image)(resources.GetObject("OpenFile.Image")));
            this.OpenFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpenFile.Name = "OpenFile";
            this.OpenFile.Size = new System.Drawing.Size(40, 22);
            this.OpenFile.Text = "Open";
            this.OpenFile.Click += new System.EventHandler(this.OpenFile_Click);
            // 
            // SaveAs
            // 
            this.SaveAs.AutoSize = false;
            this.SaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.SaveAs.Image = ((System.Drawing.Image)(resources.GetObject("SaveAs.Image")));
            this.SaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveAs.Name = "SaveAs";
            this.SaveAs.Size = new System.Drawing.Size(51, 22);
            this.SaveAs.Text = "Save As";
            this.SaveAs.Click += new System.EventHandler(this.SaveAs_Click);
            // 
            // ResetScr
            // 
            this.ResetScr.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ResetScr.Image = ((System.Drawing.Image)(resources.GetObject("ResetScr.Image")));
            this.ResetScr.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ResetScr.Name = "ResetScr";
            this.ResetScr.Size = new System.Drawing.Size(77, 22);
            this.ResetScr.Text = "Reset Screen";
            this.ResetScr.Click += new System.EventHandler(this.ResetScr_Click);
            // 
            // DeleteAllNodes
            // 
            this.DeleteAllNodes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.DeleteAllNodes.Image = ((System.Drawing.Image)(resources.GetObject("DeleteAllNodes.Image")));
            this.DeleteAllNodes.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DeleteAllNodes.Name = "DeleteAllNodes";
            this.DeleteAllNodes.Size = new System.Drawing.Size(98, 22);
            this.DeleteAllNodes.Text = "Delete All Nodes";
            this.DeleteAllNodes.Click += new System.EventHandler(this.DeleteAllNodes_Click);
            // 
            // DrawLine
            // 
            this.DrawLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.DrawLine.Image = ((System.Drawing.Image)(resources.GetObject("DrawLine.Image")));
            this.DrawLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DrawLine.Name = "DrawLine";
            this.DrawLine.Size = new System.Drawing.Size(63, 22);
            this.DrawLine.Text = "Draw Line";
            this.DrawLine.Click += new System.EventHandler(this.DrawLine_Click);
            // 
            // GenerateLines
            // 
            this.GenerateLines.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.GenerateLines.Image = ((System.Drawing.Image)(resources.GetObject("GenerateLines.Image")));
            this.GenerateLines.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.GenerateLines.Name = "GenerateLines";
            this.GenerateLines.Size = new System.Drawing.Size(85, 22);
            this.GenerateLines.Text = "Generate lines";
            this.GenerateLines.Click += new System.EventHandler(this.GenerateLines_Click);
            // 
            // DeleteLines
            // 
            this.DeleteLines.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.DeleteLines.Image = ((System.Drawing.Image)(resources.GetObject("DeleteLines.Image")));
            this.DeleteLines.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DeleteLines.Name = "DeleteLines";
            this.DeleteLines.Size = new System.Drawing.Size(86, 22);
            this.DeleteLines.Text = "Delete all lines";
            this.DeleteLines.ToolTipText = "DeleteAllLines";
            this.DeleteLines.Click += new System.EventHandler(this.DeleteAllLines_Click);
            // 
            // OpenDebug
            // 
            this.OpenDebug.AutoToolTip = false;
            this.OpenDebug.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.OpenDebug.Image = ((System.Drawing.Image)(resources.GetObject("OpenDebug.Image")));
            this.OpenDebug.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.OpenDebug.Name = "OpenDebug";
            this.OpenDebug.Size = new System.Drawing.Size(78, 22);
            this.OpenDebug.Text = "Open Debug";
            this.OpenDebug.Click += new System.EventHandler(this.OpenDebug_Click);
            // 
            // UploadFPGA
            // 
            this.UploadFPGA.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.UploadFPGA.Image = ((System.Drawing.Image)(resources.GetObject("UploadFPGA.Image")));
            this.UploadFPGA.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.UploadFPGA.Name = "UploadFPGA";
            this.UploadFPGA.Size = new System.Drawing.Size(95, 22);
            this.UploadFPGA.Text = "Upload to FPGA";
            this.UploadFPGA.Click += new System.EventHandler(this.UploadFPGA_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1279, 648);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.RotationBR);
            this.Controls.Add(this.RotationTR);
            this.Controls.Add(this.RotationBL);
            this.Controls.Add(this.RotationTL);
            this.Controls.Add(this.zoomTR);
            this.Controls.Add(this.zoomBR);
            this.Controls.Add(this.zoomBL);
            this.Controls.Add(this.zoomTL);
            this.Controls.Add(this.dried);
            this.Controls.Add(this.Right);
            this.Controls.Add(this.Front);
            this.Controls.Add(this.TopLabel);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Neon3D";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.DoubleClick += new System.EventHandler(this.Form1_DoubleClick);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseClick);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseWheel);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
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
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton OpenFile;
        private System.Windows.Forms.ToolStripButton SaveAs;
        private System.Windows.Forms.ToolStripButton ResetScr;
        private System.Windows.Forms.ToolStripButton DeleteAllNodes;
        private System.Windows.Forms.ToolStripButton DrawLine;
        private System.Windows.Forms.ToolStripButton OpenDebug;
        private System.Windows.Forms.ToolStripButton UploadFPGA;
        private System.Windows.Forms.ToolStripButton GenerateLines;
        private System.Windows.Forms.ToolStripButton DeleteLines;
    }
}

