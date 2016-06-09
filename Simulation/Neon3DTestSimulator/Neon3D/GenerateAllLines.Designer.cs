namespace Neon3D
{
    partial class GenerateAllLines
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
            this.Lines = new System.Windows.Forms.ComboBox();
            this.AmountOfLines = new System.Windows.Forms.Label();
            this.GenerateLines = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Lines
            // 
            this.Lines.FormattingEnabled = true;
            this.Lines.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8"});
            this.Lines.Location = new System.Drawing.Point(101, 40);
            this.Lines.Name = "Lines";
            this.Lines.Size = new System.Drawing.Size(63, 21);
            this.Lines.TabIndex = 0;
            // 
            // AmountOfLines
            // 
            this.AmountOfLines.AutoSize = true;
            this.AmountOfLines.Location = new System.Drawing.Point(12, 43);
            this.AmountOfLines.Name = "AmountOfLines";
            this.AmountOfLines.Size = new System.Drawing.Size(79, 13);
            this.AmountOfLines.TabIndex = 1;
            this.AmountOfLines.Text = "Amount of lines";
            // 
            // GenerateLines
            // 
            this.GenerateLines.Location = new System.Drawing.Point(28, 81);
            this.GenerateLines.Name = "GenerateLines";
            this.GenerateLines.Size = new System.Drawing.Size(122, 23);
            this.GenerateLines.TabIndex = 2;
            this.GenerateLines.Text = "Generate Lines";
            this.GenerateLines.UseVisualStyleBackColor = true;
            this.GenerateLines.Click += new System.EventHandler(this.GenerateLines_Click);
            // 
            // GenerateAllLines
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(176, 135);
            this.Controls.Add(this.GenerateLines);
            this.Controls.Add(this.AmountOfLines);
            this.Controls.Add(this.Lines);
            this.Name = "GenerateAllLines";
            this.Text = "GenerateAllLines";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox Lines;
        private System.Windows.Forms.Label AmountOfLines;
        private System.Windows.Forms.Button GenerateLines;
    }
}