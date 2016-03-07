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
        
        public double[,,] starteEndnodes = new double[100,2,3];
        
        public int nodeCounter = 0;
        public int lineCounter = 0;
        public int clicked = 0;

        public int tempStartX = 0;
        public int tempStartY = 0;

        public string whichscreenclicked = "";

        double prevx1 = 0;
        double prevy1 = 0;
        double prevx2 = 0;
        double prevy2 = 0;

        public int midPointScreenX = 960;
        public int midPointScreenY = 540;

        string topLeftScreen = "TOP";
        string topRightScreen = "FRONT";
        string bottomLeftScreen = "RIGHT";
        string bottomRightScreen = "3D";


        Form2 newForm;

        private void Form1_Load(object sender, EventArgs e)
        {
            splitScrUL.SelectedIndex = 0;
            splitScrUR.SelectedIndex = 5;
            splitScrBL.SelectedIndex = 2;
            splitScrBR.SelectedIndex = 6;

            newForm = new Form2(this); 
            newForm.Show();

        }


        private void Form1_Shown(object sender, EventArgs e)
        {
            drawAxMatrix(midPointScreenX, midPointScreenY, 255, 0, 0, 2);
            drawAxMatrix(midPointScreenX / 2, midPointScreenY / 2, 180, 168, 168, 1);
            drawAxMatrix(midPointScreenX / 2, (midPointScreenY / 2) * 3, 180, 168, 168, 1);
            drawAxMatrix((midPointScreenX / 2) * 3, midPointScreenY / 2, 180, 168, 168, 1);
            drawAxMatrix((midPointScreenX / 2) * 3, (midPointScreenY / 2) * 3, 180, 168, 168, 1);
        }

        public void drawLine(double x1, double y1, double x2, double y2, double midX, double midY, int redValue, int greenValue, int blueValue, int size, bool remove)
        {

            y1 = y1 * -1;
            y2 = y2 * -1;

            x1 = x1 + midX;
            x2 = x2 + midX;
            y1 = y1 + midY;
            y2 = y2 + midY;
            

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
            drawPixel((int)x1, (int)y1, 4, redValue, greenValue, blueValue);
            drawPixel((int)x2, (int)y2, 4, 0, 0, 0);
            prevx1 = x1;
            prevy1 = y1;
            prevx2 = x2;
            prevy2 = y2;



            //for loop for drawing beam
            for (x = (int)x1; x <= (int)x2; x++)
            {

                double y = 0;
                try
                {
                    //formula for drawing beam between nodes
                    if ((x2 - x1) == 0)
                    {
                        int count;

                        if (y2 > y1)
                        {
                            double tempY = y1;
                            y1 = y2;
                            y2 = tempY;
                        }
                        for (count = (int)y2; count <= y1; count++) {

                            if (!remove)
                            {
                                drawPixel((int)x, count, size, redValue, greenValue, blueValue);
                            }
                            else
                            {
                                removePixel((int)x, count, size);
                            }
                        }

                    } else
                    {
                        y = Math.Round(((y2 - y1) / (x2 - x1)) * (x - x1) + y1);
                    }

                }
                catch (Exception e)
                {
                    newForm.PrintDebug("Error: " + e.ToString() + " \n");
                    y = 540;
                }


                //drawing all pixels of beam between nodes
                if (y - prevY > 0 && x != 0)
                {

                    int counter;
                    for (counter = (int)prevY; counter <= (int)y; counter++)
                    {
                        if (!remove)
                        {
                            drawPixel((int)prevX, counter, size, redValue, greenValue, blueValue);
                        }
                        else
                        {
                            removePixel((int)prevX, counter, size);
                        }
                    }
                } else if (prevY - y > 0 && x != 0)
                {
                    int counter;
                    for (counter = (int)y; counter <= (int)prevY; counter++)
                    {
                        if (!remove)
                        {
                            drawPixel((int)prevX, counter, size, redValue, greenValue, blueValue);
                        }
                        else
                        {
                            removePixel((int)prevX, counter, size);
                        }
                    }
                }


                //draw line/remove prev line
                if (!remove)
                {

                    drawPixel((int)x, (int)y, size, redValue, greenValue, blueValue);
                } else
                {
                    removePixel((int)x, (int)y, size);
                }

                prevY = y;
                prevX = x;
            }


        }


        public void drawPixel(int x, int y, int size, int redValue, int greenValue, int blueValue)
        {
            Color myColor = Color.FromArgb(redValue, greenValue, blueValue);
            SolidBrush brushColor = new SolidBrush(myColor);
            Graphics g = this.CreateGraphics();
            if (x != 0 && y != 0)
            {
                g.FillRectangle(brushColor, x, y, size, size);
            }
        }

        private void removePixel(int x, int y, int size)
        {

            Brush aBrush = (Brush)Brushes.White;
            Graphics g = this.CreateGraphics();
            if (x != 0 && y != 0)
            {
                g.FillRectangle(aBrush, x, y, size, size);
            }

        }

        private void drawNodes(int midPointX, int midPointY, int conv3dto2d)
        {
            int nodesDrawn;
            double startx = 0;
            double starty = 0;
            double startz = 0;
            double endx = 0;
            double endy = 0;
            double endz = 0;
            for (nodesDrawn = 0; nodesDrawn < 200; nodesDrawn++)
            {
                if(nodesDrawn < 2)
                {
                    startx = starteEndnodes[0, 0, 0];
                    starty = starteEndnodes[0, 0, 1];
                    startz = starteEndnodes[0, 0, 2];
                    endx = starteEndnodes[0, 1, 0];
                    endy = starteEndnodes[0, 1, 1];
                    endz = starteEndnodes[0, 1, 2];

                    if(conv3dto2d == 0)
                    {
                        //top
                        starty = startz;
                        endy = endz;
                    } else if(conv3dto2d == 1)
                    {
                        //bottom
                        startx = -startx;
                        starty = startz;
                        endx = -endx;
                        endy = endz;
                    } else if (conv3dto2d == 3)
                    {
                        //back
                        startx = -startx;
                        endx = -endx;
                    } else if(conv3dto2d == 4)
                    {
                        //left
                        startx = -startz;
                        endx = -endz;
                    } else if (conv3dto2d == 5)
                    {
                        //right
                        startx = startz;
                        endx = endz;
                    }

                    drawLine(startx, starty, endx, endy, midPointX, midPointY, 0, 0, 0, 1, false);

                } else if(nodesDrawn < (starteEndnodes.Length * 2) - 2)
                {
                    int lineIndex = nodesDrawn / 2;
                    startx = starteEndnodes[lineIndex, 0, 0];
                    starty = starteEndnodes[lineIndex, 0, 1];
                    endx = starteEndnodes[lineIndex, 1, 0];
                    endy = starteEndnodes[lineIndex, 1, 1];
                    drawLine(startx, starty, endx, endy, midPointX, midPointY, 0,0,0, 1, false);

                }
                 
                
            }

        }


        public void resetScreen()
        {
            Brush aBrush = (Brush)Brushes.White;
            Graphics g = this.CreateGraphics();

            g.FillRectangle(aBrush, 0, 0, 1920, 1080);


            drawAxMatrix(midPointScreenX, midPointScreenY, 255, 0, 0, 2);
            drawAxMatrix(midPointScreenX / 2, midPointScreenY / 2, 180, 168, 168, 1);
            drawAxMatrix(midPointScreenX / 2, (midPointScreenY / 2) * 3, 180, 168, 168, 1);
            drawAxMatrix((midPointScreenX / 2) * 3, midPointScreenY / 2, 180, 168, 168, 1);
            drawAxMatrix((midPointScreenX / 2) * 3, (midPointScreenY / 2) * 3, 180, 168, 168, 1);
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {          

            newForm.PrintDebug(Convert.ToString((Cursor.Position.X - this.Left) + ":" + (Cursor.Position.Y - this.Top)) + " \n");
            //createNode((Cursor.Position.X - this.Left) - 960 , 540 - (Cursor.Position.Y - this.Top), 0);

            int splitScreenMidPosX = 0;
            int splitScreenMidPosY = 0;
            
            if (((Cursor.Position.X - this.Left) - 960) < 0 && (540 - (Cursor.Position.Y - this.Top)) > 0)
            {

                splitScreenMidPosX = 480;
                splitScreenMidPosY = 270;
                newForm.PrintDebug("upper left screen clicked \n");
                whichscreenclicked = "TopLeft";
                

            } else if(((Cursor.Position.X - this.Left) - 960) > 0 && (540 - (Cursor.Position.Y - this.Top)) > 0)
            {
                splitScreenMidPosX = 1400;
                splitScreenMidPosY = 270;
                newForm.PrintDebug("upper right screen clicked \n");
                whichscreenclicked = "TopRight";

            } else if(((Cursor.Position.X - this.Left) - 960) < 0 && (540 - (Cursor.Position.Y - this.Top)) < 0)
            {
                splitScreenMidPosX = 480;
                splitScreenMidPosY = 810;
                whichscreenclicked = "BottomLeft";
                newForm.PrintDebug("down left screen clicked \n");
            } else if (((Cursor.Position.X - this.Left) - 960) > 0 && (540 - (Cursor.Position.Y - this.Top)) < 0)
            {
                splitScreenMidPosX = 1400;
                splitScreenMidPosY = 810;
                whichscreenclicked = "BottomRight";
                newForm.PrintDebug("down right screen clicked \n");
                
            }


            clicked++;


            if (clicked == 2)
            {
                if (lineCounter > 0)
                {


                    if (whichscreenclicked == "TopLeft")
                    {
                        starteEndnodes[lineCounter, 0, 0] = tempStartX;
                        starteEndnodes[lineCounter, 0, 1] = 0;
                        starteEndnodes[lineCounter, 0, 2] = tempStartY; 
                        starteEndnodes[lineCounter, 1, 0] = (Cursor.Position.X - this.Left) - splitScreenMidPosX;
                        starteEndnodes[lineCounter, 1, 1] = splitScreenMidPosY - (Cursor.Position.Y - this.Top);
                        starteEndnodes[lineCounter, 1, 2] = 0;

                    }
                    else if (whichscreenclicked == "BottomLeft")
                    {
                        starteEndnodes[lineCounter, 0, 0] = tempStartX;
                        starteEndnodes[lineCounter, 0, 1] = tempStartY;
                        starteEndnodes[lineCounter, 0, 2] = 0;
                        starteEndnodes[lineCounter, 1, 0] = (Cursor.Position.X - this.Left) - splitScreenMidPosX;
                        starteEndnodes[lineCounter, 1, 1] = splitScreenMidPosY - (Cursor.Position.Y - this.Top);
                        starteEndnodes[lineCounter, 1, 2] = 0;

                    }

                }
                else
                {
                    starteEndnodes[lineCounter, 0, 0] = tempStartX;
                    starteEndnodes[lineCounter, 0, 1] = tempStartY;
                    starteEndnodes[lineCounter, 0, 2] = 0;
                }

                clicked = 0;
                lineCounter++;

                if (whichscreenclicked == "TopLeft")
                {
                    drawNodes(splitScreenMidPosX, splitScreenMidPosY, 0);
                }
                else if (whichscreenclicked == "BottomLeft")
                {
                    drawNodes(splitScreenMidPosX, splitScreenMidPosY, 2);
                }

            }
            else if(clicked == 1)
            {
                tempStartX = (Cursor.Position.X - this.Left) - splitScreenMidPosX;
                tempStartY = splitScreenMidPosY - (Cursor.Position.Y - this.Top);
            }
        }

        private void splitScrUL_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = splitScrUL.SelectedItem;
            topLeftScreen = selectedItem.ToString();
        }

        private void splitScrUR_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = splitScrUR.SelectedItem;
            topRightScreen = selectedItem.ToString();
        }

        private void splitScrBR_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = splitScrBR.SelectedItem;
            bottomRightScreen = selectedItem.ToString();
        }

        private void splitScrBL_SelectedIndexChanged(object sender, EventArgs e)
        {
            Object selectedItem = splitScrBL.SelectedItem;
            bottomLeftScreen = selectedItem.ToString();
        }

        public void drawAxMatrix(int midPointX, int midPointY, int r, int g, int b, int size)
        {
            drawLine(midPointX, 0, -midPointX, 0, midPointX, midPointY, r, g, b, size, false);
            drawLine(0, midPointY, 0, -midPointY, midPointX, midPointY, r, g, b, size, false);
        }
    }
}
