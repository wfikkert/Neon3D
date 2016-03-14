using System;
using System.Drawing;
using System.Threading;
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

        public static int maxLines = 100;

        public double?[,,] starteEndnodes = new double?[maxLines,2,3];
        public double?[,] allNodes = new double?[maxLines * 2, 3];
        public int?[] selectedNodes = new int?[maxLines * 2];
 
        
        public int lineCounter = 0;
        public int clicked = 0;
        public int selectedArrayLastIndex = 0;

        public int tempStartX = 0;
        public int tempStartY = 0;

        public string whichscreenclicked = "";

        double prevx1 = 0;
        double prevy1 = 0;
        double prevx2 = 0;
        double prevy2 = 0;

        public int midPointScreenX = 960;
        public int midPointScreenY = 540;

        int tempX = 0;
        int tempY = 0;
        int tempZ = 0;

        bool isYdefined = false;
        bool isXdefined = false;
        bool isZdefined = false;

        int rawClickedXpos = 0;
        int rawClickedYpos = 0;




        Form2 newForm;
        Thread wekeepdrawing;

        private void Form1_Load(object sender, EventArgs e)
        {

            newForm = new Form2(this); 
            newForm.Show();

            wekeepdrawing = new Thread(new ThreadStart(keepdrawingScreens));
            wekeepdrawing.Start();
        }


        private void Form1_Shown(object sender, EventArgs e)
        {
            resetScreen();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            wekeepdrawing.Abort();
        }

        public void keepdrawingScreens()
        {

            while (true)
            {
                drawNodes(midPointScreenX / 2, midPointScreenY / 2, 0); // top left "TOP"
                drawNodes((midPointScreenX / 2) * 3, midPointScreenY / 2, 4); //top right "RIGHT"
                drawNodes(midPointScreenX / 2, (midPointScreenY / 2) * 3, 2); //bottom left "FRONT"
                drawNodes((midPointScreenX / 2) * 3, (midPointScreenY / 2) * 3, 6); // bottom right "3D"
                Thread.Sleep(50);
            }
            
        }

        public void resetNodes()
        {
            starteEndnodes = new double?[maxLines, 2, 3];
            allNodes = new double?[maxLines * 2, 3];
            selectedNodes = new int?[maxLines * 2];

            lineCounter = 0;
            clicked = 0;
            selectedArrayLastIndex = 0;
            resetScreen();
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
            removePixel((int)prevx1, (int)prevy1, 1);
            removePixel((int)prevx2, (int)prevy2, 1);
            drawPixel((int)x1, (int)y1, 1, redValue, greenValue, blueValue);
            drawPixel((int)x2, (int)y2, 1, 0, 0, 0);
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
            int linesDrawn;
            double startx = 0;
            double starty = 0;
            double startz = 0;
            double endx = 0;
            double endy = 0;
            double endz = 0;



            for (int i = 0; i < (allNodes.Length / 3); i++)
            {
                if(allNodes[i, 0] != null && allNodes[i,1] != null)
                {

                    startx = allNodes[i, 0].Value;
                    starty = allNodes[i, 1].Value;
                    if (conv3dto2d == 0)
                    {
                        starty = allNodes[i, 2].Value;
                    }
                    else if (conv3dto2d == 1)
                    {
                        startx = -allNodes[i, 0].Value; ;
                        starty = allNodes[i, 2].Value; ;
                    }
                    else if (conv3dto2d == 3)
                    {
                        startx = -allNodes[i, 0].Value; ;
                    }
                    else if (conv3dto2d == 4)
                    {
                        startx = allNodes[i, 2].Value; ;
                    }
                    else if (conv3dto2d == 5)
                    {
                        startx = -allNodes[i, 2].Value; ;
                    }


                    for(int selectedcounter = 0; selectedcounter < selectedNodes.Length; selectedcounter++)
                    {
                        if(selectedNodes[selectedcounter] != null)
                        {
                            if (selectedNodes[selectedcounter].Value == i)
                            {
                                drawPixel((int)startx + midPointX, midPointY - (int)starty, 4, 0, 255, 0);
                                drawPixel((int)startx + midPointX, midPointY - (int)starty, 4, 0, 255, 0);
                                break;
                            }
                            else
                            {
                                drawPixel((int)startx + midPointX, midPointY - (int)starty, 4, 255, 0, 0);
                                drawPixel((int)startx + midPointX, midPointY - (int)starty, 4, 255, 0, 0);
                            }
                        } else
                        {
                            drawPixel((int)startx + midPointX, midPointY - (int)starty, 4, 255, 0, 0);
                            drawPixel((int)startx + midPointX, midPointY - (int)starty, 4, 255, 0, 0);
                        }
                       

                    }
                    
                }
                
                       
                    
            }


            for (linesDrawn = 0; linesDrawn < maxLines; linesDrawn++)
            {
               if(starteEndnodes[linesDrawn, 0, 0] != null && starteEndnodes[linesDrawn, 0, 1] != null && starteEndnodes[linesDrawn, 1, 0] != null && starteEndnodes[linesDrawn, 1, 1] != null)
                {
                    startx = starteEndnodes[linesDrawn, 0, 0].Value;
                    starty = starteEndnodes[linesDrawn, 0, 1].Value;
                    startz = starteEndnodes[linesDrawn, 0, 2].Value;
                    endx = starteEndnodes[linesDrawn, 1, 0].Value;
                    endy = starteEndnodes[linesDrawn, 1, 1].Value;
                    endz = starteEndnodes[linesDrawn, 1, 2].Value;


                    if (conv3dto2d == 0)
                    {
                        starty = startz;
                        endy = endz;
                    }
                    else if (conv3dto2d == 1)
                    {
                        startx = -startx;
                        starty = startz;
                        endx = -endx;
                        endy = endz;
                    }
                    else if (conv3dto2d == 3)
                    {
                        startx = -startx;
                        endx = -endx;
                    }
                    else if (conv3dto2d == 4)
                    {
                        startx = startz;
                        endx = endz;
                    }
                    else if (conv3dto2d == 5)
                    {
                        startx = -startz;
                        endx = -endz;
                    }
                    drawLine(startx, starty, endx, endy, midPointX, midPointY, 0, 0, 0, 1, false);
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

            
            //createNode((Cursor.Position.X - this.Left) - 960 , 540 - (Cursor.Position.Y - this.Top), 0);

            int splitScreenMidPosX = 0;
            int splitScreenMidPosY = 0;

            rawClickedXpos = (Cursor.Position.X - this.Left) - 10;
            rawClickedYpos = (Cursor.Position.Y - this.Top) - 32;
            int clickedX =  rawClickedXpos - 960;
            int clickedY = 540 - rawClickedYpos;


            if (clickedX < 0 && clickedY > 0)
            {

                splitScreenMidPosX = 480;
                splitScreenMidPosY = 270;
                newForm.PrintDebug("upper left screen clicked \n");
                whichscreenclicked = "TopLeft";
                

            } else if(clickedX > 0 && clickedY > 0)
            {
                splitScreenMidPosX = 1400;
                splitScreenMidPosY = 270;
                newForm.PrintDebug("upper right screen clicked \n");
                whichscreenclicked = "TopRight";

            } else if(clickedX < 0 && clickedY < 0)
            {
                splitScreenMidPosX = 480;
                splitScreenMidPosY = 810;
                whichscreenclicked = "BottomLeft";
                newForm.PrintDebug("down left screen clicked \n");
            } else if (clickedX > 0 && clickedY < 0)
            {
                splitScreenMidPosX = 1400;
                splitScreenMidPosY = 810;
                whichscreenclicked = "BottomRight";
                newForm.PrintDebug("down right screen clicked \n");
                
            }

            int localScreenClickedX = rawClickedXpos - splitScreenMidPosX;
            int localScreenClickedY = splitScreenMidPosY - rawClickedYpos;

          
            

            if (whichscreenclicked == "TopLeft" && !isXdefined && !isZdefined)
            {
                tempZ = localScreenClickedY;
                tempX = localScreenClickedX;
                allNodes[clicked, 0] = tempX;
                allNodes[clicked, 1] = 0;
                allNodes[clicked, 2] = tempZ;
                isXdefined = true;
                isZdefined = true;
                isYdefined = false;
                
            }
            else if(isZdefined && isXdefined && whichscreenclicked == "BottomLeft")
            {
                tempY = localScreenClickedY;
                isXdefined = false;
                isZdefined = false;
                isYdefined = true;

                removePixel(tempX + splitScreenMidPosX, splitScreenMidPosY - (int)allNodes[clicked, 1].Value, 4);
                removePixel(tempZ + (splitScreenMidPosX * 3), (splitScreenMidPosY / 3) - (int)allNodes[clicked, 1].Value, 4);
                allNodes[clicked, 1] = tempY;


                newForm.PrintDebug("NODE CREATED AT-> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                tempX = 0;
                tempY = 0;
                tempZ = 0;

                clicked++;
            }
            else if (isZdefined && isXdefined && whichscreenclicked == "TopRight")
            {
                tempY = localScreenClickedY;
                isXdefined = false;
                isZdefined = false;
                isYdefined = true;

                removePixel(tempZ + splitScreenMidPosX + 40, splitScreenMidPosY - (int)allNodes[clicked, 1].Value, 4);
                removePixel(tempX + (splitScreenMidPosX / 3) + 14, (splitScreenMidPosY * 3) - (int)allNodes[clicked, 1].Value, 4);
                allNodes[clicked, 1] = tempY;


                

                newForm.PrintDebug("NODE CREATED AT-> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                tempX = 0;
                tempY = 0;
                tempZ = 0;

                clicked++;
            }
            else if(isZdefined && isXdefined && !isYdefined )
            {
                MessageBox.Show("You need to define the Y position of your node", "Hint");
            }


            

        }

        

        public void createLines()
        {
            

                for (int i = 0; i < selectedNodes.Length; i++)
                {
                    if ((i % 2) == 0)
                    {
                        if (selectedNodes[i] != null && selectedNodes[i + 1] != null)
                        {
                            starteEndnodes[lineCounter, 0, 0] = allNodes[selectedNodes[i].Value, 0];
                            starteEndnodes[lineCounter, 0, 1] = allNodes[selectedNodes[i].Value, 1];
                            starteEndnodes[lineCounter, 0, 2] = allNodes[selectedNodes[i].Value, 2];
                            starteEndnodes[lineCounter, 1, 0] = allNodes[selectedNodes[i + 1].Value, 0];
                            starteEndnodes[lineCounter, 1, 1] = allNodes[selectedNodes[i + 1].Value, 1];
                            starteEndnodes[lineCounter, 1, 2] = allNodes[selectedNodes[i + 1].Value, 2];
                            lineCounter++;


                            drawPixel((int)allNodes[selectedNodes[i].Value, 0] + (midPointScreenX / 2), (midPointScreenY / 2) - (int)allNodes[selectedNodes[i].Value, 2], 4, 255, 0, 0);
                            drawPixel((int)allNodes[selectedNodes[i + 1].Value, 0] + (midPointScreenX / 2), (midPointScreenY / 2) - (int)allNodes[selectedNodes[i + 1].Value, 2], 4, 255, 0, 0);

                            drawPixel((int)allNodes[selectedNodes[i].Value, 0] + (midPointScreenX / 2), ((midPointScreenY / 2) * 3) - (int)allNodes[selectedNodes[i].Value, 1], 4, 255, 0, 0);
                            drawPixel((int)allNodes[selectedNodes[i + 1].Value, 0] + (midPointScreenX / 2), ((midPointScreenY / 2) * 3) - (int)allNodes[selectedNodes[i + 1].Value, 1], 4, 255, 0, 0);

                            selectedNodes = new int?[maxLines * 2];
                            selectedArrayLastIndex = 0;
                            break;
                        }
                        else
                        {
                            MessageBox.Show("You need to select 2 nodes!!!", "Hint");

                            break;
                        }
                    }
                }

            

            
        }

        public void drawAxMatrix(int midPointX, int midPointY, int r, int g, int b, int size)
        {
            drawLine(midPointX, 0, -midPointX, 0, midPointX, midPointY, r, g, b, size, false);
            drawLine(0, midPointY, 0, -midPointY, midPointX, midPointY, r, g, b, size, false);
        }


       
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            int splitScreenMidPosX = 0;
            int splitScreenMidPosY = 0;

            rawClickedXpos = (Cursor.Position.X - this.Left) - 10;
            rawClickedYpos = (Cursor.Position.Y - this.Top) - 32;
            int clickedX = rawClickedXpos - 960;
            int clickedY = 540 - rawClickedYpos;


            if (clickedX < 0 && clickedY > 0)
            {

                splitScreenMidPosX = 480;
                splitScreenMidPosY = 270;
                newForm.PrintDebug("upper left screen clicked \n");
                whichscreenclicked = "TopLeft";


            }
            else if (clickedX > 0 && clickedY > 0)
            {
                splitScreenMidPosX = 1400;
                splitScreenMidPosY = 270;
                newForm.PrintDebug("upper right screen clicked \n");
                whichscreenclicked = "TopRight";

            }
            else if (clickedX < 0 && clickedY < 0)
            {
                splitScreenMidPosX = 480;
                splitScreenMidPosY = 810;
                whichscreenclicked = "BottomLeft";
                newForm.PrintDebug("down left screen clicked \n");
            }
            else if (clickedX > 0 && clickedY < 0)
            {
                splitScreenMidPosX = 1400;
                splitScreenMidPosY = 810;
                whichscreenclicked = "BottomRight";
                newForm.PrintDebug("down right screen clicked \n");

            }

            int localScreenClickedX = rawClickedXpos - splitScreenMidPosX;
            int localScreenClickedY = splitScreenMidPosY - rawClickedYpos;

            newForm.PrintDebug(Convert.ToString(localScreenClickedX) + ":" + Convert.ToString((localScreenClickedY)) + " \n");

            if (!isXdefined && isYdefined && !isZdefined)
            {
                newForm.PrintDebug("AMOUNT OF INDEXES: " + allNodes.Length / 3 + "\n");
                for (int i = 0; i < (allNodes.Length / 3); i++)
                {

                   
                        if ((localScreenClickedX > allNodes[i, 0] - 8 &&
                            localScreenClickedX < allNodes[i, 0] + 8) &&
                            (localScreenClickedY > allNodes[i, 2] - 8 &&
                            localScreenClickedY < allNodes[i, 2] + 8))
                        {

                            newForm.PrintDebug("NODE WITH INDEX: " + i+ " SELECTED!\n");
                            selectedNodes[selectedArrayLastIndex] = i;
                            selectedArrayLastIndex++;
                        }
                    
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                createLines();
            } else if (e.KeyData == Keys.R)
            {
                resetScreen();
            } else if (e.KeyData == (Keys.Control | Keys.R))
            {
                resetNodes();
                resetScreen();
            }
        }
    }
}
