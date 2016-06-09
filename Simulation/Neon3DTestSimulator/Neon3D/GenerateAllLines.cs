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
    public partial class GenerateAllLines : Form
    {
        public Drawer drawer;
        public Form1 mainform;
        
        public GenerateAllLines(Drawer drawer, Form mainform)
        {
            InitializeComponent();
            this.drawer = drawer;
            this.mainform = mainform as Form1;
            Lines.Text = "1";
            
        }
        
        private void GenerateLines_Click(object sender, EventArgs e)
        {

            drawer.DeleteAllLines();
            drawer.GenerateAllLines(int.Parse(Lines.Text));
            mainform.resetScreen();
            this.Close();
        }
    }
}
