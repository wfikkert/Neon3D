using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            Debug.AppendText(tekst);
        }

        private void drawLines_Click(object sender, EventArgs e)
        {
            mainForm.createLines();
        }
    }
}
