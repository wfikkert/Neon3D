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

        double[,] nodes = new double[7, 2];

        double prevx1 = 0;
        double prevy1 = 0;
        double prevx2 = 0;
        double prevy2 = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Show3D_Click(object sender, EventArgs e)
        {
            if (rmvPrevBeam.Checked)
            {
                drawLine(Int32.Parse(previousCor[0]), Int32.Parse(previousCor[1]), Int32.Parse(previousCor[2]), Convert.ToDouble(previousCor[3]), true);
            }
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


        void createNode(double x, double y, double z) {
            try
            {
                Debug.AppendText("Added node: x: " + x + ", y: " + y + ", z: " + z + " on index " + (nodes.Length - 1) + " \n");                
                nodes[nodes.Length - 1, 0] = x;
                nodes[nodes.Length - 1, 1] = y;
                nodes[nodes.Length - 1, 2] = z;
            } catch
            {
                Debug.AppendText("Could not add node \n");
            }
        }

        private void drawLine(double x1, double y1, double x2, double y2, bool remove)
        {
            
            y1 = y1 * -1;
            y2 = y2 * -1;

            x1 = x1 + 960;
            x2 = x2 + 960;
            y1 = y1 + 540;
            y2 = y2 + 540;

            Debug.Text = "";
            Debug.AppendText("Start drawline \n");

            //switch points when first point is behind second point on x axel
            if (x2 < x1)
            {
                double tempX = x1;
                double tempY = y1;
                x1 = x2;
                y1 = y2;

                x2 = tempX;
                y2 = tempY;
            }
            


            //setup vars for loop
            double x;
            double prevY = 0;
            double prevX = 0;

            //remove and redraw nodes for begin and end
            removePixel((int)prevx1, (int)prevy1, 4);
            removePixel((int)prevx2, (int)prevy2, 4);
            drawPixel((int)x1, (int)y1, 4);
            drawPixel((int)x2, (int)y2, 4);
            prevx1 = x1;
            prevy1 = y1;
            prevx2 = x2;
            prevy2 = y2;

            
            
            //for loop for drawing beam
            for(x = (int)x1; x <= (int)x2; x++)
            {
               
                double y = 0;
                try
                {
                    //formula for drawing beam between nodes
                    if((x2 - x1) == 0)
                    {
                        int count;

                        if(y2 > y1)
                        {
                            double tempY = y1;
                            y1 = y2;
                            y2 = tempY;
                        }
                        for (count = (int)y2; count <= y1; count++) {

                            if (!remove)
                            {
                                drawPixel((int)x, count, 1);
                            }
                            else
                            {
                                removePixel((int)x, count, 1);
                            }
                        }
                        
                    } else
                    {
                        y = Math.Round(((y2 - y1) / (x2 - x1)) * (x - x1) + y1);
                    }
                                      
                }
                catch(Exception e)
                {
                    Debug.AppendText("Error: " + e.ToString() + " \n");
                    y = 540;
                }
                
               
                //drawing all pixels of beam between nodes
                if(y - prevY > 0 && x != 0)
                {
                     
                    int counter;
                    for(counter = (int)prevY; counter <= (int)y; counter++)
                    {
                        if (!remove)
                        {
                            drawPixel((int)prevX, counter, 1);
                        }
                        else
                        {
                            removePixel((int)prevX, counter, 1);
                        }
                    }
                } else if(prevY - y > 0 && x != 0)
                {
                    int counter;
                    for (counter = (int)y; counter <= (int)prevY; counter++)
                    {
                        if (!remove)
                        {
                            drawPixel((int)prevX, counter, 1);
                        }
                        else
                        {
                            removePixel((int)prevX, counter, 1);
                        }
                    }
                }


                //draw line/remove prev line
                if (!remove)
                {
                    Debug.AppendText("draw pixel on coord: x: " + x + ", y: " + y + "\n");
                    drawPixel((int)x, (int)y, 1);
                } else
                {
                    removePixel((int)x, (int)y, 1);
                }

                prevY = y;
                prevX = x;
            }
            

        }
        

        private void resetScreen()
        {
            Brush aBrush = (Brush)Brushes.White;
            Graphics g = this.CreateGraphics();

            g.FillRectangle(aBrush, 0, 0, 1920, 1080);
        }

        private void drawPixel(int x, int y, int size)
        {
            Brush aBrush = (Brush)Brushes.Black;
            Graphics g = this.CreateGraphics();
            if (x != 0 && y != 0)
            {
                g.FillRectangle(aBrush, x, y, size, size);
            }
        }

        private void removePixel(int x, int y, int size)
        {

            Brush aBrush = (Brush)Brushes.White;
            Graphics g = this.CreateGraphics();
            if(x != 0 && y != 0)
            {
                g.FillRectangle(aBrush, x, y, size, size);
            }
           
        }

        private void ResetScr_Click(object sender, EventArgs e)
        {
            resetScreen();
        }
    }
}
