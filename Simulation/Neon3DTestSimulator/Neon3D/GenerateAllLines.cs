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
    //class for generate lines with closest neighbour algorithm.
    public partial class GenerateAllLines : Form
    {
        public Drawer drawer;
        public Form1 mainform;

        //initializes everything.
        public GenerateAllLines(Drawer drawer, Form mainform)
        {
            InitializeComponent();
            this.drawer = drawer;
            this.mainform = mainform as Form1;
            //sets combobox to default value '1', so you can't sent null value.
            Lines.Text = "1";

        }

        //cals the function GenerateAllLines from drawer class, and the value you selected in the combobox will be the amount of lines drawn from each node.
        //this only works for .obj files.
        //first all previous lines will be deleted.
        private void GenerateLines_Click(object sender, EventArgs e)
        {

            drawer.DeleteAllLines();
            drawer.GenerateAllLines(int.Parse(Lines.Text));
            mainform.resetScreen();
            this.Close();
        }
    }
}
