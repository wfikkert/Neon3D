using System;
using System.Drawing;
using System.Windows.Forms;

namespace Neon3D
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string[] previousCor = { "0", "0", "0", "0" };

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Show3D_Click(object sender, EventArgs e)
        {

            drawLine(Int32.Parse(previousCor[0]), Int32.Parse(previousCor[1]), Int32.Parse(previousCor[2]), Convert.ToDouble(previousCor[3]), true);
            string cor;
            string[] corArray = { "0", "0", "0", "0"};
            if ((cor = Coordinates.Text) != "")
            {
                Debug.AppendText("Coordinates set:  " + cor + "\n");
                corArray = cor.Split(',');
                previousCor = corArray;
            }
            
            drawLine((double)Int32.Parse(corArray[0]), (double)Int32.Parse(corArray[1]), (double)Int32.Parse(corArray[2]), Convert.ToDouble(corArray[3]), false);
        }

        private void drawLine(double x1, double y1, double x2, double y2, bool remove)
        {
            Debug.AppendText("Start drawline \n");
            double x;
            double prevY = 0;
            double prevX = 0;
            for(x = x1; x <= x2; x++)
            {

                //double y = ((y2 / x2) - (y1 / y2)) * x + y1;
                double y;
                try
                {
                    Debug.AppendText("running calculation \n");
                    y = ((y2 - y1) / x2) * (x - x1) + y1;
                }
                catch
                {
                    y = 0;
                }
                
                y = Math.Round(y);
                if(y - prevY > 0 && x != 0)
                {
                     
                    int counter;
                    for(counter = (int)prevY; counter <= (int)y; counter++)
                    {
                        if (!remove)
                        {
                            drawPixel((int)prevX, counter);
                        }
                        else
                        {
                            removePixel((int)prevX, counter);
                        }
                    }
                }

                if (!remove)
                {
                    Debug.AppendText("DRAW PIXEL AT: " + (int)x + ", " + (int) y + " \n");
                    drawPixel((int)x, (int)y);
                } else
                {
                    removePixel((int)x, (int)y);
                }

                prevY = y;
                prevX = x;
            }
            

        }
        

        private void drawPixel(int x, int y)
        {
            Brush aBrush = (Brush)Brushes.Black;
            Graphics g = this.CreateGraphics();

            g.FillRectangle(aBrush, x, y, 1, 1);
        }

        private void removePixel(int x, int y)
        {

            Brush aBrush = (Brush)Brushes.White;
            Graphics g = this.CreateGraphics();
            if(x != 0 && y != 0)
            {
                g.FillRectangle(aBrush, x, y, 1, 1);
            }
           
        }
    }
}
