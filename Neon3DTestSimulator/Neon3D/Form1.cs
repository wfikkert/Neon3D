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


        public int whichScreenSelected { get; set; } // 0= top left, 1= top right, 2= bottom left, 3= bottom right

        string[] previousCor = { "0", "0", "0", "0" };

        public int clicked { get; set; }

        public double zoomTopLeft = 1;
        public double zoomTopRight = 1;
        public double zoomBottomLeft = 1;
        public double zoomBottomRight = 1;

        public int[] fullScrMP = new int[2];
        public int[] topLeftMP = new int[2];
        public int[] topRightMP = new int[2];
        public int[] bottomLeftMP = new int[2];
        public int[] bottomRightMP = new int[2];
        public int[][] screenInformation = new int[5][];
        public bool zoomed = false;

        public int tempStartX = 0;
        public int tempStartY = 0;

        double prevx1 = 0;
        double prevy1 = 0;
        double prevx2 = 0;
        double prevy2 = 0;

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
        Thread wekeeptrackofourcursor;
        Drawer drawer;

        private void Form1_Load(object sender, EventArgs e)
        {

            fullScrMP[0] = 960;
            fullScrMP[1] = 540;
            topLeftMP[0] =      fullScrMP[0] / 2;
            topLeftMP[1] =      fullScrMP[1] / 2;
            topRightMP[0] =     (fullScrMP[0] / 2) * 3;
            topRightMP[1] =     fullScrMP[1] / 2;
            bottomLeftMP[0] =   fullScrMP[0] / 2;
            bottomLeftMP[1] =   (fullScrMP[1] / 2) * 3;
            bottomRightMP[0] =  (fullScrMP[0] / 2) * 3;
            bottomRightMP[1] =  (fullScrMP[1] / 2) * 3;
            screenInformation[0] = fullScrMP;
            screenInformation[1] = topLeftMP;
            screenInformation[2] = topRightMP;
            screenInformation[3] = bottomLeftMP;
            screenInformation[4] = bottomLeftMP;

            drawer = new Drawer(this,debugCallback,100,screenInformation);
            newForm = new Form2(this); 
            newForm.Show();

            wekeeptrackofourcursor = new Thread(new ThreadStart(() => checkCursorScreenPosition(screenInformation[0][0], screenInformation[0][1])));
            wekeeptrackofourcursor.Start();
            wekeepdrawing = new Thread(new ThreadStart(keepdrawingScreens));
            wekeepdrawing.Start();

        }

        private void debugCallback(string msg)
        {
            newForm.PrintDebug(msg);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            resetScreen();
        }

        private void checkCursorScreenPosition(int midpointx, int midpointy)
        {
            while (true)
            {
                rawClickedXpos = (Cursor.Position.X - this.Left) - 10;
                rawClickedYpos = (Cursor.Position.Y - this.Top) - 32;
                int clickedX = rawClickedXpos - midpointx;
                int clickedY = midpointy - rawClickedYpos;
                if (clickedX < 0 && clickedY > 0)
                {
                    //topleft
                    whichScreenSelected = 1;
                }
                else if (clickedX > 0 && clickedY > 0)
                {
                    //topright
                    whichScreenSelected = 2;
                }
                else if (clickedX < 0 && clickedY < 0)
                {
                    //bottomleft
                    whichScreenSelected = 3;
                }
                else if (clickedX > 0 && clickedY < 0)
                {
                    //bottomright;
                    whichScreenSelected = 4;
                }
                Thread.Sleep(13);
            }
            
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {

            switch (whichScreenSelected)
            {
                case 1: //top left
                    if (e.Delta > 0)
                    {
                        zoomTopLeft = zoomTopLeft + 0.1;
                    }
                    else if (e.Delta < 0)
                    {
                        if (zoomTopLeft > 0.1)
                        {
                            zoomTopLeft = zoomTopLeft - 0.1;
                        }
                    }

                    zoomTL.Text = "Zoom: " + zoomTopLeft + "x";
                    newForm.PrintDebug("upper left screen scrolled, \n ZOOM: " + zoomTopLeft + "\n");
                    break;

                case 2: //top right
                    if (e.Delta > 0)
                    {
                        zoomTopRight = zoomTopRight + 0.1;
                    }
                    else if (e.Delta < 0)
                    {
                        if (zoomTopRight > 0.1)
                        {
                            zoomTopRight = zoomTopRight - 0.1;
                        }
                    }

                    zoomTR.Text = "Zoom: " + zoomTopRight + "x";
                    newForm.PrintDebug("upper right screen scrolled, \n ZOOM: " + zoomTopRight + "\n");
                    break;

                case 3://bottom left
                    if (e.Delta > 0)
                    {
                        zoomBottomLeft = zoomBottomLeft + 0.1;
                    }
                    else if (e.Delta < 0)
                    {
                        if (zoomBottomLeft > 0.1)
                        {
                            zoomBottomLeft = zoomBottomLeft - 0.1;
                        }
                    }
                    zoomBL.Text = "Zoom: " + zoomBottomLeft + "x";
                    newForm.PrintDebug("down left screen scrolled, \n ZOOM: " + zoomBottomLeft + "\n");
                    break;

                case 4://bottom right
                    if (e.Delta > 0)
                    {
                        zoomBottomRight = zoomBottomRight + 0.1;
                    }
                    else if (e.Delta < 0)
                    {
                        if (zoomBottomRight > 0.1)
                        {
                            zoomBottomRight = zoomBottomRight - 0.1;
                        }
                    }

                    zoomBR.Text = "Zoom: " + zoomBottomRight + "x";
                    newForm.PrintDebug("down right screen scrolled, \n ZOOM: " + zoomBottomRight + "\n");
                    break;
            }
            zoomed = true;
            
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
                drawer.drawNodes(0, 1, zoomTopLeft); // top left "TOP"
                drawer.drawNodes(4, 2, zoomTopRight); //top right "RIGHT"
                drawer.drawNodes(2, 3, zoomBottomLeft); //bottom left "FRONT"
                drawer.drawNodes(6, 4, zoomBottomRight); // bottom right "3D"
            }
            
        }

        public void resetNodes()
        {
            drawer.starteEndnodes = new double?[drawer.maxLines, 2, 3];
            drawer.allNodes = new double?[drawer.maxLines * 2, 3];
            drawer.selectedNodes = new int?[drawer.maxLines * 2];

            drawer.lineCounter = 0;
            drawer.selectedArrayLastIndex = 0;
            clicked = 0;
            resetScreen();
        }

        public void resetScreen()
        {
            Brush aBrush = (Brush)Brushes.White;
            Graphics g = this.CreateGraphics();

            g.FillRectangle(aBrush, 0, 0, 1920, 1080);

            drawer.drawAxMatrix(0, 255, 0, 0, 2);
            drawer.drawAxMatrix(1, 180, 168, 168, 1);
            drawer.drawAxMatrix(2, 180, 168, 168, 1);
            drawer.drawAxMatrix(3, 180, 168, 168, 1);
            drawer.drawAxMatrix(4, 180, 168, 168, 1);
        }
        
        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            //createNode((Cursor.Position.X - this.Left) - 960 , 540 - (Cursor.Position.Y - this.Top), 0);

            int localScreenClickedX = rawClickedXpos - screenInformation[whichScreenSelected][0];
            int localScreenClickedY = screenInformation[whichScreenSelected][1] - rawClickedYpos;
            switch (whichScreenSelected)
            {
                case 1: //top left
                    if (!isXdefined && !isZdefined)
                    {
                        tempZ = localScreenClickedY;
                        tempX = localScreenClickedX;
                        drawer.allNodes[clicked, 0] = tempX;
                        drawer.allNodes[clicked, 1] = 0;
                        drawer.allNodes[clicked, 2] = tempZ;
                        isXdefined = true;
                        isZdefined = true;
                        isYdefined = false;
                        newForm.PrintDebug("START NEW NODE (x,z) \n AT -> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                    }
                    else if (isZdefined && isYdefined)
                    {
                        tempX = localScreenClickedX;
                        isXdefined = true;
                        isZdefined = false;
                        isYdefined = false;

                        drawer.removePixel(tempZ + screenInformation[0][0] + 40, screenInformation[0][1] - (int)drawer.allNodes[clicked, 1].Value, 4);
                        drawer.removePixel(tempX + (screenInformation[0][0] / 3) + 14, (screenInformation[0][1] * 3) - (int)drawer.allNodes[clicked, 1].Value, 4);
                        drawer.allNodes[clicked, 0] = tempX;

                        newForm.PrintDebug("NODE CREATED NODE CREATED IN TOP LEFT SCREEN(x) \n AT-> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                        tempX = 0;
                        tempY = 0;
                        tempZ = 0;

                        clicked++;
                    }
                    break;
                case 2: //top right
                    if (isZdefined && isXdefined)
                    {
                        tempY = localScreenClickedY;
                        isXdefined = false;
                        isZdefined = false;
                        isYdefined = true;

                        drawer.removePixel(tempZ + screenInformation[0][0] + 40, screenInformation[0][1] - (int)drawer.allNodes[clicked, 1].Value, 4);
                        drawer.removePixel(tempX + (screenInformation[0][0] / 3) + 14, (screenInformation[0][1] * 3) - (int)drawer.allNodes[clicked, 1].Value, 4);
                        drawer.allNodes[clicked, 1] = tempY;

                        newForm.PrintDebug("NODE CREATED IN TOP RIGHT SCREEN(y) \n AT-> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                        tempX = 0;
                        tempY = 0;
                        tempZ = 0;

                        clicked++;
                    }
                    else if (!isYdefined && !isZdefined)
                    {
                        tempZ = localScreenClickedX;
                        tempY = localScreenClickedY;
                        drawer.allNodes[clicked, 0] = 0;
                        drawer.allNodes[clicked, 1] = tempY;
                        drawer.allNodes[clicked, 2] = tempZ;
                        isXdefined = false;
                        isZdefined = true;
                        isYdefined = true;

                        newForm.PrintDebug("START NEW NODE (y,z) \n AT -> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                    }
                    
                    break;
                case 3: //bottom left
                    if (isZdefined && isXdefined)
                    {
                        tempY = localScreenClickedY;
                        isXdefined = false;
                        isZdefined = false;
                        isYdefined = true;

                        drawer.removePixel(tempX + screenInformation[0][0], screenInformation[0][1] - (int)drawer.allNodes[clicked, 1].Value, 4);
                        drawer.removePixel(tempZ + (screenInformation[0][0] * 3), (screenInformation[0][1] / 3) - (int)drawer.allNodes[clicked, 1].Value, 4);
                        drawer.allNodes[clicked, 1] = tempY;


                        newForm.PrintDebug("NODE CREATED IN BOTTOM LEFT SCREEN(y) \n AT-> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                        tempX = 0;
                        tempY = 0;
                        tempZ = 0;

                        clicked++;
                    }else if (isZdefined && isYdefined)
                    {
                        tempX = localScreenClickedX;
                        isXdefined = true;
                        isZdefined = false;
                        isYdefined = false;

                        drawer.removePixel(tempX + screenInformation[0][0], screenInformation[0][1] - (int)drawer.allNodes[clicked, 1].Value, 4);
                        drawer.removePixel(tempZ + (screenInformation[0][0] * 3), (screenInformation[0][1] / 3) - (int)drawer.allNodes[clicked, 1].Value, 4);
                        drawer.allNodes[clicked, 0] = tempX;


                        newForm.PrintDebug("NODE CREATED IN BOTTOM LEFT SCREEN(x) \n AT -> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                        tempX = 0;
                        tempY = 0;
                        tempZ = 0;

                        clicked++;
                    }
                    break;
                case 4: //bottom right
                    break;
            }
            
        }
        
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {

            int customRawClickedXpos = rawClickedXpos - 10;
            int customRawClickedYpos = rawClickedYpos - 32;
            int clickedX = rawClickedXpos - screenInformation[0][0];
            int clickedY = screenInformation[0][1] - rawClickedYpos;

            int localScreenClickedX = rawClickedXpos - screenInformation[whichScreenSelected][0];
            int localScreenClickedY = screenInformation[whichScreenSelected][1] - rawClickedYpos;

            newForm.PrintDebug(Convert.ToString(localScreenClickedX) + ":" + Convert.ToString((localScreenClickedY)) + " \n");

            if ((!isXdefined && isYdefined && !isZdefined) || (isXdefined && !isYdefined && !isZdefined))
            {
                newForm.PrintDebug("AMOUNT OF INDEXES: " + drawer.allNodes.Length / 3 + "\n");
                for (int i = 0; i < (drawer.allNodes.Length / 3); i++)
                {

                   
                        if ((localScreenClickedX > drawer.allNodes[i, 0] - 8 &&
                            localScreenClickedX < drawer.allNodes[i, 0] + 8) &&
                            (localScreenClickedY > drawer.allNodes[i, 2] - 8 &&
                            localScreenClickedY < drawer.allNodes[i, 2] + 8))
                        {

                            newForm.PrintDebug("NODE WITH INDEX: " + i+ " SELECTED!\n");
                            drawer.selectedNodes[drawer.selectedArrayLastIndex] = i;
                            drawer.selectedArrayLastIndex++;
                        }
                    
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                drawer.createLines();
            } else if (e.KeyData == Keys.R)
            {
                resetScreen();
            } else if (e.KeyData == (Keys.Control | Keys.R))
            {
                resetNodes();
                resetScreen();
            }
        }

        public void createLines()
        {
            drawer.createLines();
        }
    }
}
