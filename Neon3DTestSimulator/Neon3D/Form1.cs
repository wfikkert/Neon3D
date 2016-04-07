using System;
using System.Drawing;
using System.IO;
using System.Text;
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

        public int[] rotationTopLeft = new int[2];
        public int[] rotationTopRight = new int[2];
        public int[] rotationBottomLeft = new int[2];
        public int[] rotationBottomRight = new int[2];

        

        public int[] fullScrMP = new int[2];
        public int[] topLeftMP = new int[2];
        public int[] topRightMP = new int[2];
        public int[] bottomLeftMP = new int[2];
        public int[] bottomRightMP = new int[2];
        public int[][] screenInformation = new int[5][];
        public bool zoomed = false;
        public bool upKeyIsPressed = false;
        public bool downKeyIsPressed = false;
        public bool leftKeyIsPressed = false;
        public bool rightKeyIsPressed = false;
        public int rotation = 0;

        public int tempStartX = 0;
        public int tempStartY = 0;

        int tempX = 0;
        int tempY = 0;
        int tempZ = 0;

        int rawClickedXpos = 0;
        int rawClickedYpos = 0;




        public static Form2 newForm;
        Thread wekeepdrawing;
        Thread wekeeptrackofourcursor;
        Thread wekeeptrackofourrotation;
        Drawer drawer;

        private void Form1_Load(object sender, EventArgs e)
        {
            int wHeight = Screen.PrimaryScreen.Bounds.Height / 2; 
            int wWidth = Screen.PrimaryScreen.Bounds.Width / 2;

            fullScrMP[0] = wWidth;
            fullScrMP[1] = wHeight;
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
            rotationTopLeft[0] = 0;
            rotationTopLeft[1] = 0;
            rotationTopRight[0] = 0;
            rotationTopRight[1] = 0;
            rotationBottomLeft[0] = 0;
            rotationBottomLeft[1] = 0;
            rotationBottomRight[0] = 0;
            rotationBottomRight[1] = 0;

            drawer = new Drawer(this,debugCallback,100,screenInformation);
            newForm = new Form2(this);


            try
            {
                if (File.Exists("piramide.n3d"))
                {
                    FileStream myStream = new FileStream("piramide.n3d", FileMode.Open);

                    if (myStream != null)
                    {
                        using (myStream)
                        {
                            // Insert code to read the stream here.
                            StreamReader myReader = new StreamReader(myStream);
                            using (myReader)
                            {
                                string data = myReader.ReadToEnd();
                                setArrays(data);
                            }
                        }
                    }
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }

            wekeeptrackofourcursor = new Thread(new ThreadStart(() => checkCursorScreenPosition(screenInformation[0][0], screenInformation[0][1])));
            wekeeptrackofourcursor.Start();
            wekeepdrawing = new Thread(new ThreadStart(keepdrawingScreens));
            wekeepdrawing.Start();
            wekeeptrackofourrotation = new Thread(new ThreadStart(() => keepTrackOfRotation()));
            wekeeptrackofourrotation.Start();

        }
        public string generateString()
        {
            return drawer.generateString();
        }

        public void setArrays(string array)
        {
            drawer.setArrays(array);
        }

        private void debugCallback(string msg)
        {
            newForm.PrintDebug(msg);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            resetScreen();
        }
        
        private void keepTrackOfRotation()
        {
            while (true)
            {
                switch (whichScreenSelected)
                {
                    case 1: //top left
                        if (rightKeyIsPressed)
                        {
                            rightKeyIsPressed = false;
                            leftKeyIsPressed = false;
                            
                            if (rotationTopLeft[0] == 361)
                            {
                                rotationTopLeft[0] = 0;
                            }
                            rotationTopLeft[0]++;
                        }
                        else if (leftKeyIsPressed)
                        {
                            rightKeyIsPressed = false;
                            leftKeyIsPressed = false;
                            if (rotationTopLeft[0] == -1)
                            {
                                rotationTopLeft[0] = 360;
                            }
                            rotationTopLeft[0]--;
                        }

                        if (upKeyIsPressed)
                        {
                            upKeyIsPressed = false;
                            downKeyIsPressed = false;

                            if (rotationTopLeft[1] == 361)
                            {
                                rotationTopLeft[1] = 0;
                            }
                            rotationTopLeft[1]++;
                        }
                        else if (downKeyIsPressed)
                        {
                            upKeyIsPressed = false;
                            downKeyIsPressed = false;
                            if (rotationTopLeft[1] == -1)
                            {
                                rotationTopLeft[1] = 360;
                            }
                            rotationTopLeft[1]--;
                        }
                        try
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                RotationTL.Text = "RotationX,Y: " + rotationTopLeft[0].ToString() + ", " + rotationTopLeft[1].ToString();
                            });
                        }
                        catch { }



                        break;

                    case 2: //top right
                        if (rightKeyIsPressed)
                        {
                            rightKeyIsPressed = false;
                            leftKeyIsPressed = false;
                            if (rotationTopRight[0] == 361)
                            {
                                rotationTopRight[0] = 0;
                            }
                            rotationTopRight[0]++;
                        }
                        else if (leftKeyIsPressed)
                        {
                            rightKeyIsPressed = false;
                            leftKeyIsPressed = false;
                            if (rotationTopRight[0] == -1)
                            {
                                rotationTopRight[0] = 360;
                            }
                            rotationTopRight[0]--;
                        }

                        if (upKeyIsPressed)
                        {
                            upKeyIsPressed = false;
                            downKeyIsPressed = false;

                            if (rotationTopLeft[1] == 361)
                            {
                                rotationTopLeft[1] = 0;
                            }
                            rotationTopLeft[1]++;
                        }
                        else if (downKeyIsPressed)
                        {
                            upKeyIsPressed = false;
                            downKeyIsPressed = false;
                            if (rotationTopRight[1] == -1)
                            {
                                rotationTopRight[1] = 360;
                            }
                            rotationTopRight[1]--;
                        }
                        try
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                RotationTR.Text = "RotationX,Y: " + rotationTopRight[0].ToString() + ", " + rotationTopRight[1].ToString();
                            });
                        }
                        catch { } 

                        break;

                    case 3://bottom left
                        if (rightKeyIsPressed)
                        {
                            rightKeyIsPressed = false;
                            leftKeyIsPressed = false;
                            if (rotationBottomLeft[0] == 361)
                            {
                                rotationBottomLeft[0] = 0;
                            }
                            rotationBottomLeft[0]++;
                        }
                        else if (leftKeyIsPressed)
                        {
                            rightKeyIsPressed = false;
                            leftKeyIsPressed = false;
                            if (rotationBottomLeft[0] == -1)
                            {
                                rotationBottomLeft[0] = 360;
                            }
                            rotationBottomLeft[0]--;
                        }

                        if (upKeyIsPressed)
                        {
                            upKeyIsPressed = false;
                            downKeyIsPressed = false;

                            if (rotationBottomLeft[1] == 361)
                            {
                                rotationBottomLeft[1] = 0;
                            }
                            rotationBottomLeft[1]++;
                        }
                        else if (downKeyIsPressed)
                        {
                            upKeyIsPressed = false;
                            downKeyIsPressed = false;
                            if (rotationBottomLeft[1] == -1)
                            {
                                rotationBottomLeft[1] = 360;
                            }
                            rotationBottomLeft[1]--;
                        }

                        try
                        {
                            this.Invoke((MethodInvoker)delegate {
                                RotationBL.Text = "RotationX,Y: " + rotationBottomLeft[0].ToString() + ", " + rotationBottomLeft[1].ToString();
                            });
                        }
                        catch (Exception ex) { }

                        
                        break;

                    case 4://bottom right
                        if (rightKeyIsPressed)
                        {
                            rightKeyIsPressed = false;
                            leftKeyIsPressed = false;
                            if (rotationBottomRight[0] == 361)
                            {
                                rotationBottomRight[0] = 0;
                            }
                            rotationBottomRight[0]++;
                        }
                        else if (leftKeyIsPressed)
                        {
                            rightKeyIsPressed = false;
                            leftKeyIsPressed = false;
                            if (rotationBottomRight[0] == -1)
                            {
                                rotationBottomRight[0] = 360;
                            }
                            rotationBottomRight[0]--;
                        }

                        if (upKeyIsPressed)
                        {
                            upKeyIsPressed = false;
                            downKeyIsPressed = false;

                            if (rotationBottomRight[1] == 361)
                            {
                                rotationBottomRight[1] = 0;
                            }
                            rotationBottomRight[1]++;
                        }
                        else if (downKeyIsPressed)
                        {
                            upKeyIsPressed = false;
                            downKeyIsPressed = false;
                            if (rotationBottomRight[1] == -1)
                            {
                                rotationBottomRight[1] = 360;
                            }
                            rotationBottomRight[1]--;
                        }
                        try
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                RotationBR.Text = "RotationX,Y: " + rotationBottomRight[0].ToString() + ", " + rotationBottomRight[1].ToString();
                            });
                        }
                        catch { }
                        break;
                }
                Thread.Sleep(100);
            }
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

                drawer.drawNodes(2, 3, zoomBottomLeft, rotationBottomLeft); //bottom left "FRONT"
                drawer.drawNodes(0, 1, zoomTopLeft, rotationTopLeft); // top left "TOP"
                drawer.drawNodes(4, 2, zoomTopRight, rotationTopRight); //top right "RIGHT"
                drawer.drawNodes(6, 4, zoomBottomRight, rotationBottomRight); // bottom right "3D"
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
            zoomBottomLeft = 1;
            zoomBottomRight = 1;
            zoomTopLeft = 1;
            zoomTopRight = 1;
            rotationTopLeft[0] = 0;
            rotationTopLeft[1] = 0;
            rotationTopRight[0] = 0;
            rotationTopRight[1] = 0;
            rotationBottomLeft[0] = 0;
            rotationBottomLeft[1] = 0;
            rotationBottomRight[0] = 0;
            rotationBottomRight[1] = 0;
            RotationTL.Text = "Rotation: " + rotationTopLeft.ToString();
            RotationTR.Text = "Rotation: " + rotationTopRight.ToString();
            RotationBL.Text = "Rotation: " + rotationBottomLeft.ToString();
            RotationBR.Text = "Rotation: " + rotationBottomRight.ToString();
            zoomBL.Text = "Zoom: " + zoomBottomLeft.ToString();
            zoomBR.Text = "Zoom: " + zoomBottomRight.ToString();
            zoomTR.Text = "Zoom: " + zoomTopRight.ToString();
            zoomTL.Text = "Zoom: " + zoomTopLeft.ToString();

            drawer.drawAxMatrix(0, 255, 0, 0, 6);
            drawInsideAxles(4);
        }

        public void drawInsideAxles(int size) {
            drawer.drawAxMatrix(1, 180, 168, 168, size);
            drawer.drawAxMatrix(2, 180, 168, 168, size);
            drawer.drawAxMatrix(3, 180, 168, 168, size);
            drawer.drawAxMatrix(4, 180, 168, 168, size);
        }


        public int previousScreenSelected = 0;
        public bool newNode = true;
        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            //createNode((Cursor.Position.X - this.Left) - 960 , 540 - (Cursor.Position.Y - this.Top), 0);

            int localScreenClickedX = rawClickedXpos - screenInformation[whichScreenSelected][0];
            int localScreenClickedY = screenInformation[whichScreenSelected][1] - rawClickedYpos;

            newForm.PrintDebug("PREV SCREEN SELECTED: " + previousScreenSelected + "\n");
            switch (whichScreenSelected)
            {
                case 1: //top left
                    if (newNode)
                    {
                        newNode = false;
                        tempZ = localScreenClickedY;
                        tempX = localScreenClickedX;
                        drawer.allNodes[clicked, 0] = tempX;
                        drawer.allNodes[clicked, 1] = 0;
                        drawer.allNodes[clicked, 2] = tempZ;

                        newForm.PrintDebug("START NEW NODE (x,z) \n AT -> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");

                        previousScreenSelected = whichScreenSelected;
                    }
                    else if (previousScreenSelected == 2)
                    {
                        tempX = localScreenClickedX;
                        
                        drawer.allNodes[clicked, 0] = tempX;
                        drawInsideAxles(4);
                        newForm.PrintDebug("NODE CREATED NODE CREATED IN TOP LEFT SCREEN(x) \n AT-> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                        tempX = 0;
                        tempY = 0;
                        tempZ = 0;
                        newNode = true;
                        clicked++;
                    }
                    else if (previousScreenSelected == 3)
                    {
                        tempZ = localScreenClickedY;
                        drawer.allNodes[clicked, 2] = tempZ;

                        drawInsideAxles(4);
                        newForm.PrintDebug("NODE CREATED IN TOP RIGHT SCREEN(y) \n AT-> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                        tempX = 0;
                        tempY = 0;
                        tempZ = 0;
                        newNode = true;
                        clicked++;
                    } else
                    {
                        MessageBox.Show("You need to define Y in front or right view!", "Hint");
                    }
                    break;
                case 2: //top right
                    if (newNode)
                    {
                        tempZ = localScreenClickedX;
                        tempY = localScreenClickedY;
                        newNode = false;
                        drawer.allNodes[clicked, 0] = 0;
                        drawer.allNodes[clicked, 1] = tempY;
                        drawer.allNodes[clicked, 2] = tempZ;

                        previousScreenSelected = whichScreenSelected;
                    }
                    else if (previousScreenSelected == 1)
                    {
                        tempY = localScreenClickedY;
                        drawer.allNodes[clicked, 1] = tempY;
                        drawInsideAxles(4);
                        newForm.PrintDebug("NODE CREATED IN TOP RIGHT SCREEN(y) \n AT-> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                        tempX = 0;
                        tempY = 0;
                        tempZ = 0;
                        newNode = true;
                        clicked++;

                    } else if(previousScreenSelected == 3)
                    {
                        tempZ = localScreenClickedX;
                        drawer.allNodes[clicked, 2] = tempZ;
                        drawInsideAxles(4);
                        newForm.PrintDebug("NODE CREATED IN TOP RIGHT SCREEN(y) \n AT-> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                        tempX = 0;
                        tempY = 0;
                        tempZ = 0;
                        newNode = true;
                        clicked++;
                    } else
                    {
                        MessageBox.Show("You need to define X in front or top view!", "Hint");
                    }
                    
                    break;
                case 3: //bottom left
                    if (newNode) {
                        tempY = localScreenClickedY;
                        tempX = localScreenClickedX;
                        newNode = false;
                        drawer.allNodes[clicked, 0] = tempX;
                        drawer.allNodes[clicked, 1] = tempY;
                        drawer.allNodes[clicked, 2] = 0;

                        newForm.PrintDebug("START NEW NODE (x,z) \n AT -> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");

                        previousScreenSelected = whichScreenSelected;
                    }
                    else if (previousScreenSelected == 1)
                    {
                        tempY = localScreenClickedY;
                        
                        drawer.allNodes[clicked, 1] = tempY;

                        drawInsideAxles(4);
                        newForm.PrintDebug("NODE CREATED IN BOTTOM LEFT SCREEN(y) \n AT-> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                        tempX = 0;
                        tempY = 0;
                        tempZ = 0;
                        newNode = true;
                        clicked++;
                    }else if (previousScreenSelected == 2)
                    {
                        tempX = localScreenClickedX;
                        
                        drawer.allNodes[clicked, 0] = tempX;

                        drawInsideAxles(4);
                        newForm.PrintDebug("NODE CREATED IN BOTTOM LEFT SCREEN(x) \n AT -> X: " + tempX + ", Y: " + tempY + ", Z: " + tempZ + "\n");
                        tempX = 0;
                        tempY = 0;
                        tempZ = 0;
                        newNode = true;
                        clicked++;
                    } else
                    {
                        MessageBox.Show("You need to define Z in top or right view!", "Hint");
                    }
                    break;
                case 4: //bottom right
                    break;
            }

        }
        
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {

            int customRawClickedXpos = rawClickedXpos;
            int customRawClickedYpos = rawClickedYpos;
            int clickedX = rawClickedXpos - screenInformation[0][0];
            int clickedY = screenInformation[0][1] - rawClickedYpos;

            int localScreenClickedX = rawClickedXpos - screenInformation[whichScreenSelected][0];
            int localScreenClickedY = screenInformation[whichScreenSelected][1] - rawClickedYpos;

            newForm.PrintDebug(Convert.ToString(localScreenClickedX) + ":" + Convert.ToString((localScreenClickedY)) + " \n");

            int range = 8;
            try
            {
                newForm.PrintDebug("AMOUNT OF INDEXES: " + drawer.allNodes.Length / 3 + "\n");
                for (int i = 0; i < (drawer.allNodes.Length / 3); i++)
                {
                    

                    if (((localScreenClickedX > drawer.allNodes[i, 0] - range &&
                        localScreenClickedX < drawer.allNodes[i, 0] + range) &&
                        (localScreenClickedY > drawer.allNodes[i, 2] - range &&
                        localScreenClickedY < drawer.allNodes[i, 2] + range)) || (
                        (localScreenClickedX > drawer.allNodes[i, 0] - range &&
                        localScreenClickedX < drawer.allNodes[i, 0] + range) &&
                        (localScreenClickedY > drawer.allNodes[i, 1] - range &&
                        localScreenClickedY < drawer.allNodes[i, 1] + range)) || 
                        ((localScreenClickedY > drawer.allNodes[i, 1] - range &&
                        localScreenClickedY < drawer.allNodes[i, 1] + range) &&
                        (localScreenClickedX > drawer.allNodes[i, 2] - range &&
                        localScreenClickedX < drawer.allNodes[i, 2] + range)))
                    {

                        newForm.PrintDebug("NODE WITH INDEX: " + i + " SELECTED!\n");
                        drawer.selectedNodes[drawer.selectedArrayLastIndex] = i;
                        drawer.selectedArrayLastIndex++;
                    } 

                }
            }
            catch(Exception ex)
            {
                newForm.PrintDebug("ERROR: " + ex.ToString() + "\n");
            }
                
              
        }
        
        private void refreshScreen()
        {
            Brush aBrush = (Brush)Brushes.White;
            Graphics g = this.CreateGraphics();

            g.FillRectangle(aBrush, 0, 0, 1920, 1080);

            drawer.drawAxMatrix(0, 255, 0, 0, 6);
            drawInsideAxles(4);
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
            } else if (e.KeyData == Keys.Right)
            {
                
                leftKeyIsPressed = false;
                rightKeyIsPressed = true;
            } else if (e.KeyData == Keys.Left)
            {

                rightKeyIsPressed = false;
                leftKeyIsPressed = true;
            }else if(e.KeyData == Keys.Up)
            {
                upKeyIsPressed = true;
                downKeyIsPressed = false;

            }else if(e.KeyData == Keys.Down)
            {
                upKeyIsPressed = false;
                downKeyIsPressed = true;
            } else if(e.KeyData == Keys.NumPad6)
            {
                rotationTopLeft[0] = 90;
                refreshScreen();
            }
            else if (e.KeyData == Keys.NumPad4)
            {
                rotationTopLeft[0] = 270;
                refreshScreen();
            }
            else if (e.KeyData == Keys.NumPad8)
            {
                rotationTopLeft[0] = 0;
                refreshScreen();
            }
            else if (e.KeyData == Keys.NumPad2)
            {
                rotationTopLeft[0] = 180;
                refreshScreen();
            }
        }

        public void createLines()
        {
            drawer.createLines();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Right)
            {
                while (drawer.isStillDrawing) ;
                refreshScreen();
                leftKeyIsPressed = false;
                rightKeyIsPressed = false;
            }
            else if (e.KeyData == Keys.Left)
            {
                while (drawer.isStillDrawing) ;
                refreshScreen();
                rightKeyIsPressed = false;
                leftKeyIsPressed = false;
            }else if(e.KeyData == Keys.Up)
            {
                while (drawer.isStillDrawing) ;
                refreshScreen();
                upKeyIsPressed = false;
                downKeyIsPressed = false;
            }else if(e.KeyData == Keys.Down)
            {
                while (drawer.isStillDrawing) ;
                refreshScreen();

                upKeyIsPressed = false;
                downKeyIsPressed = false;
            }
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
                                setArrays(data);
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
                newForm.PrintDebug("SAVING FILE TO: " + name + "\n");
                using (FileStream fs = File.Create(name))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(generateString());
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }
            }
        }

        private void ResetScr_Click(object sender, EventArgs e)
        {
            resetScreen();
        }

        private void DeleteAllNodes_Click(object sender, EventArgs e)
        {
            resetNodes();
        }

        private void DrawLine_Click(object sender, EventArgs e)
        {
            createLines();
        }

        private void OpenDebug_Click(object sender, EventArgs e)
        {
            try
            {
                newForm.Show();
            }
            catch
            {
                newForm = new Form2(this);
                newForm.Show();
            }
            
        }
    }
}
