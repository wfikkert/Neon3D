using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neon3D
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private Form1 mainForm = null;
        public Form2(Form callingForm)
        {
            mainForm = callingForm as Form1;
            InitializeComponent();
        }


        private void Debug_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void ResetScr_Click(object sender, EventArgs e)
        {
            mainForm.resetScreen();
        }

        private void resetNodes_Click(object sender, EventArgs e)
        {
            mainForm.resetNodes();
        }

        public void PrintDebug(string tekst)
        {
            try
            {
                if (InvokeRequired)
                {
                    this.Invoke(new Action<string>(Debug.AppendText), new object[] { tekst });
                    return;
                }
                else
                {
                    Debug.AppendText(tekst);
                }
            } catch
            {

            }
           
        }

        private void drawLines_Click(object sender, EventArgs e)
        {
            mainForm.createLines();
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            StreamReader myReader = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            
            openFileDialog1.Filter = "Node3D Files (*.n3d*)|*.n3d*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            // Insert code to read the stream here.
                            myReader = new StreamReader(myStream);
                            using (myReader)
                            {
                                string data = myReader.ReadToEnd();
                                mainForm.setArrays(data);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        
        private void SaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Node3D Files (*.n3d*)|*.n3d*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string name = saveFileDialog1.FileName;
                PrintDebug("SAVING FILE TO: " + name + "\n");
                using (FileStream fs = File.Create(name))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(mainForm.generateString());
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }
            }
        }

        
       
    }
}
