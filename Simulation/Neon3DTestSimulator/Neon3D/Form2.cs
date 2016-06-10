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
    //class for printing debug text in a textbox.
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

        //prints message in textbox.
        public void PrintDebug(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    this.Invoke(new Action<string>(Debug.AppendText), new object[] { text });
                    return;
                }
                else
                {
                    Debug.AppendText(text);
                }
            }
            catch
            {
            }

        }
    }
}
