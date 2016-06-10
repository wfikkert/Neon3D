using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO.Ports;

namespace Neon3D
{
    //class for the mainscreen.
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //global variables
        public bool zoomed = false;
        //checks which screen the mousecursor is.
        public int whichScreenSelected { get; set; } // 0= top left, 1= top right, 2= bottom left, 3= bottom right
        public int previousScreenSelected = 0;

        //checks in which screen is clicked
        public int clicked { get; set; }

        //zoom for each screen
        public float zoomTopLeft = 1;
        public float zoomTopRight = 1;
        public float zoomBottomLeft = 1;
        public float zoomBottomRight = 1;

        //rotation is only necessary for the 3D screen
        public int[] rotationBottomRight = new int[3];

        //initializing of the screen.
        public int[] fullScrMP = new int[2];
        public int[] topLeftMP = new int[2];
        public int[] topRightMP = new int[2];
        public int[] bottomLeftMP = new int[2];
        public int[] bottomRightMP = new int[2];
        public int[][] screenInformation = new int[5][];

        //check which key is pressed.
        public static bool upKeyIsPressed = false; //+y
        public static bool downKeyIsPressed = false; //-y
        public static bool leftKeyIsPressed = false; //+x
        public static bool rightKeyIsPressed = false; //-x
        public static bool ctrlLeftKeyIsPressed = false; //+z
        public static bool ctrlRightKeyIsPressed = false; //-z

        //global value where the mouse is located
        int rawClickedXpos = 0;
        int rawClickedYpos = 0;

        //main form width and height
        public int wHeight;
        public int wWidth;

        //global threads to perform in background
        Thread wekeepdrawing;
        Thread wekeeptrackofourcursor;
        Thread wekeeptrackofourrotation;

        //initializing the comport.
        public SerialPort comPort = new SerialPort();
        public bool received = false;

        //bool to know if a new node needs to be created or if a current node needs its last coordinate
        public bool newNode = true;

        // opens debug form
        public static Form2 newForm;
        //drawer class init
        Drawer drawer;

        int tempX = 0;
        int tempY = 0;
        int tempZ = 0;

        //everything in here starts while the program starts.
        private void Form1_Load(object sender, EventArgs e)
        {
            string[] names = SerialPort.GetPortNames();
            //checks if comport is available (if so it opens the comport else it shows a message)
            if (names.Length != 0 && !comPort.IsOpen)
            {
                comPort.PortName = names[0];
                comPort.BaudRate = 115000;
                comPort.Handshake = 0;
                comPort.Open();
                comPort.RtsEnable = true;
                comPort.DtrEnable = true;
                comPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            }
            else
            {
                MessageBox.Show("FPGA not found, connect FPGA via RS-232 to your laptop by USB and restart Node3D.");
            }

            wHeight = Screen.PrimaryScreen.Bounds.Height / 2;
            wWidth = Screen.PrimaryScreen.Bounds.Width / 2;
            //changes to the best resolution.
            fullScrMP[0] = wWidth;
            fullScrMP[1] = wHeight;
            //calculates where the 4 screens have to be placed.
            topLeftMP[0] = fullScrMP[0] / 2;
            topLeftMP[1] = fullScrMP[1] / 2;
            topRightMP[0] = (fullScrMP[0] / 2) * 3;
            topRightMP[1] = fullScrMP[1] / 2;
            bottomLeftMP[0] = fullScrMP[0] / 2;
            bottomLeftMP[1] = (fullScrMP[1] / 2) * 3;
            bottomRightMP[0] = (fullScrMP[0] / 2) * 3;
            bottomRightMP[1] = (fullScrMP[1] / 2) * 3;

            //sets the midpoint for each view
            screenInformation[0] = fullScrMP;
            screenInformation[1] = topLeftMP;
            screenInformation[2] = topRightMP;
            screenInformation[3] = bottomLeftMP;
            screenInformation[4] = bottomRightMP;
            //sets rotation to 0
            rotationBottomRight[0] = 0;
            rotationBottomRight[1] = 0;
            rotationBottomRight[2] = 0;

            //sets locatoin for text to be written
            TopLabel.Location = new System.Drawing.Point(13, 39);
            Front.Location = new System.Drawing.Point(13, wHeight + 13);
            Right.Location = new System.Drawing.Point(wWidth + 13, 39);
            dried.Location = new System.Drawing.Point(wWidth + 13, wHeight + 13); // dried =drieD = 3D 
            zoomTL.Location = new System.Drawing.Point(13, 52);
            zoomBL.Location = new System.Drawing.Point(13, wHeight + 28);
            zoomBR.Location = new System.Drawing.Point(wWidth + 13, wHeight + 28);
            zoomTR.Location = new System.Drawing.Point(wWidth + 13, 52);
            RotationBR.Location = new System.Drawing.Point(wWidth + 13, wHeight + 41);

            drawer = new Drawer(this, debugCallback, 433, screenInformation);
            //initializing debug form.
            newForm = new Form2(this);

            //checks if piramide.n3d is available, if so then it opens the file.
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
            //starting threads.
            wekeeptrackofourcursor = new Thread(new ThreadStart(() => checkCursorScreenPosition(screenInformation[0][0], screenInformation[0][1])));
            wekeeptrackofourcursor.Start();
            wekeepdrawing = new Thread(new ThreadStart(keepdrawingScreens));
            wekeepdrawing.Start();
            wekeeptrackofourrotation = new Thread(new ThreadStart(() => keepTrackOfRotation()));
            wekeeptrackofourrotation.Start();

        }

        //if loading is done the screen will reset so everything is shown correctly.
        private void Form1_Shown(object sender, EventArgs e)
        {
            resetScreen();
        }

        //if you shut down the application it will abort all threads.
        protected override void OnFormClosing(FormClosingEventArgs e)
        {

            base.OnFormClosing(e);
            wekeepdrawing.Abort();
            wekeeptrackofourcursor.Abort();
            wekeeptrackofourrotation.Abort();

        }

        //see drawer.cs
        public string generateString()
        {
            return drawer.generateString();
        }
        //see drawer.cs
        public string generateStringFPGA()
        {
            return drawer.generateStringFPGA();
        }
        //see drawer.cs
        public void setArrays(string array)
        {
            drawer.setArrays(array);
        }
        //see Form2.cs
        private void debugCallback(string msg)
        {
            newForm.PrintDebug(msg);
        }

        //checks if the keys are still pressed, increase or decrease rotation if so and send a value to the FPGA if it is connected.
        private void keepTrackOfRotation()
        {
            while (true)
            {
                if (rightKeyIsPressed)
                {
                    rightKeyIsPressed = false;
                    leftKeyIsPressed = false;

                    if (comPort.IsOpen)
                    {
                        comPort.Write("r");
                    }
                    if (rotationBottomRight[0] == 360)
                    {
                        rotationBottomRight[0] = 0;
                    }
                    rotationBottomRight[0]++;
                }
                else if (leftKeyIsPressed)
                {
                    rightKeyIsPressed = false;
                    leftKeyIsPressed = false;
                    if (comPort.IsOpen)
                    {
                        comPort.Write("l");
                    }
                    if (rotationBottomRight[0] == 0)
                    {
                        rotationBottomRight[0] = 360;
                    }
                    rotationBottomRight[0]--;
                }

                if (upKeyIsPressed)
                {
                    upKeyIsPressed = false;
                    downKeyIsPressed = false;
                    if (comPort.IsOpen)
                    {
                        comPort.Write("u");
                    }
                    if (rotationBottomRight[1] == 360)
                    {
                        rotationBottomRight[1] = 0;
                    }
                    rotationBottomRight[1]++;
                }
                else if (downKeyIsPressed)
                {
                    upKeyIsPressed = false;
                    downKeyIsPressed = false;
                    if (comPort.IsOpen)
                    {
                        comPort.Write("b");
                    }
                    if (rotationBottomRight[1] == 0)
                    {
                        rotationBottomRight[1] = 360;
                    }
                    rotationBottomRight[1]--;
                }


                if (ctrlLeftKeyIsPressed)
                {
                    ctrlLeftKeyIsPressed = false;
                    ctrlRightKeyIsPressed = false;
                    if (comPort.IsOpen)
                    {
                        comPort.Write("z");
                    }
                    if (rotationBottomRight[2] == 360)
                    {
                        rotationBottomRight[2] = 0;
                    }
                    rotationBottomRight[2]++;
                }
                else if (ctrlRightKeyIsPressed)
                {
                    ctrlLeftKeyIsPressed = false;
                    ctrlRightKeyIsPressed = false;
                    if (comPort.IsOpen)
                    {
                        comPort.Write("x");
                    }
                    if (rotationBottomRight[2] == 0)
                    {
                        rotationBottomRight[2] = 360;
                    }
                    rotationBottomRight[2]--;
                }
                try
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        //changes the text of rotation
                        RotationBR.Text = "RotationX,Y,Z: " + rotationBottomRight[0].ToString() + ", " + rotationBottomRight[1].ToString() + ", " + rotationBottomRight[2].ToString();
                    });
                }
                catch { }
                Thread.Sleep(100);
            }
        }
        //checks in which screen the cursor is located.
        private void checkCursorScreenPosition(int midpointx, int midpointy)
        {
            while (true)
            {
                //substract 10 and 32 because we wanted it at the top of the cursor.
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


        //continuesly draws the 3d model on all screens and views
        public void keepdrawingScreens()
        {
            while (true)
            {
                drawer.drawNodesAndBeams(0, 1, zoomTopLeft, new int[] { 0, 0, 0 }); // top left "TOP"
                drawer.drawNodesAndBeams(4, 2, zoomTopRight, new int[] { 0, 0, 0 }); //top right "RIGHT"
                drawer.drawNodesAndBeams(2, 3, zoomBottomLeft, new int[] { 0, 0, 0 }); //bottom left "FRONT"
                drawer.drawNodesAndBeams(6, 4, zoomBottomRight, rotationBottomRight); // bottom right "3D"
            }
        }

        //clears all node arrays
        public void resetNodes()
        {
            drawer.starteEndnodes = new float?[drawer.maxLines, 2, 3];
            drawer.allNodes = new float?[drawer.maxLines * 2, 3];
            drawer.selectedNodes = new int?[drawer.maxLines * 2];

            drawer.lineCounter = 0;
            drawer.selectedArrayLastIndex = 0;
            clicked = 0;
            resetScreen();
        }

        // resets the screen and information on the screen by drawing a white space over everything
        public void resetScreen()
        {
            Brush aBrush = (Brush)Brushes.White;
            Graphics g = this.CreateGraphics();

            g.FillRectangle(aBrush, 0, 0, wWidth * 2, wHeight * 2);
            zoomBottomLeft = 1;
            zoomBottomRight = 1;
            zoomTopLeft = 1;
            zoomTopRight = 1;
            rotationBottomRight[0] = 0;
            rotationBottomRight[1] = 0;
            rotationBottomRight[2] = 0;
            RotationBR.Text = "RotationX,Y,Z: " + rotationBottomRight[0].ToString() + ", " + rotationBottomRight[1].ToString() + ", " + rotationBottomRight[2].ToString();
            zoomBL.Text = "Zoom: " + zoomBottomLeft.ToString();
            zoomBR.Text = "Zoom: " + zoomBottomRight.ToString();
            zoomTR.Text = "Zoom: " + zoomTopRight.ToString();
            zoomTL.Text = "Zoom: " + zoomTopLeft.ToString();

            drawer.drawAxMatrix(0, 255, 0, 0, 6);
            drawInsideAxles(4);
        }

        //draws axis on screen to devide the screen
        public void drawInsideAxles(int size)
        {
            drawer.drawAxMatrix(1, 180, 168, 168, size);
            drawer.drawAxMatrix(2, 180, 168, 168, size);
            drawer.drawAxMatrix(3, 180, 168, 168, size);
        }
        //on mouswheel move: check which screen selecrted and update zoom for that screen
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {

            switch (whichScreenSelected)
            {
                case 1: //top left
                    if (e.Delta > 0)
                    {
                        if (zoomTopLeft < 1.5)
                        {
                            zoomTopLeft = zoomTopLeft + (float)0.1;
                        }

                    }
                    else if (e.Delta < 0)
                    {
                        //zooming works now until zoom is 0.1
                        if (zoomTopLeft > 0.2)
                        {
                            zoomTopLeft = zoomTopLeft - (float)0.1;
                        }
                    }

                    zoomTL.Text = "Zoom: " + zoomTopLeft + "x";
                    newForm.PrintDebug("upper left screen scrolled, \n ZOOM: " + zoomTopLeft + "\n");
                    break;

                case 2: //top right
                    if (e.Delta > 0)
                    {
                        if (zoomTopRight < 1.5)
                        {
                            zoomTopRight = zoomTopRight + (float)0.1;
                        }

                    }
                    else if (e.Delta < 0)
                    {
                        if (zoomTopRight > 0.2)
                        {
                            zoomTopRight = zoomTopRight - (float)0.1;
                        }
                    }

                    zoomTR.Text = "Zoom: " + zoomTopRight + "x";
                    newForm.PrintDebug("upper right screen scrolled, \n ZOOM: " + zoomTopRight + "\n");
                    break;

                case 3://bottom left
                    if (e.Delta > 0)
                    {
                        if (zoomBottomLeft < 1.5)
                        {
                            zoomBottomLeft = zoomBottomLeft + (float)0.1;
                        }

                    }
                    else if (e.Delta < 0)
                    {
                        if (zoomBottomLeft > 0.2)
                        {
                            zoomBottomLeft = zoomBottomLeft - (float)0.1;
                        }
                    }
                    zoomBL.Text = "Zoom: " + zoomBottomLeft + "x";
                    newForm.PrintDebug("down left screen scrolled, \n ZOOM: " + zoomBottomLeft + "\n");
                    break;

                case 4://bottom right
                    if (e.Delta > 0)
                    {
                        if (zoomBottomRight < 1.5)
                        {
                            zoomBottomRight = zoomBottomRight + (float)0.1;
                        }

                    }
                    else if (e.Delta < 0)
                    {
                        if (zoomBottomRight > 0.2)
                        {
                            zoomBottomRight = zoomBottomRight - (float)0.1;
                        }
                    }

                    zoomBR.Text = "Zoom: " + zoomBottomRight + "x";
                    newForm.PrintDebug("down right screen scrolled, \n ZOOM: " + zoomBottomRight + "\n");
                    break;
            }
            zoomed = true;

        }

        //on doubleclick: check which screen and create corresponding node
        private void Form1_DoubleClick(object sender, EventArgs e)
        {

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
                    }
                    else
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

                    }
                    else if (previousScreenSelected == 3)
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
                    }
                    else
                    {
                        MessageBox.Show("You need to define X in front or top view!", "Hint");
                    }

                    break;
                case 3: //bottom left
                    if (newNode)
                    {
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
                    }
                    else if (previousScreenSelected == 2)
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
                    }
                    else
                    {
                        MessageBox.Show("You need to define Z in top or right view!", "Hint");
                    }
                    break;
                case 4: //bottom right
                    break;
            }
        }

        //refreshes screen by removing all drawn objects on screen
        public void refreshScreen()
        {
            Brush aBrush = (Brush)Brushes.White;
            Graphics g = this.CreateGraphics();

            g.FillRectangle(aBrush, wWidth + 6, wHeight + 6, wWidth * 2, wHeight * 2);
        }

        //executes method in drawer.cs for creating line
        public void createLines()
        {
            drawer.createLines();
        }

        //mouse click event handler, checks if location where clicked is location where node exists, if so, add it to selected nodes array
        //if selected, remove it from selected node array
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
                        bool isAlreadySelected = false;
                        int a;
                        for (a = 0; a < (3); a++)
                        {
                            if (drawer.selectedNodes[a] == i)
                            {
                                isAlreadySelected = true;
                                break;
                            }
                        }
                        if (!isAlreadySelected)
                        {
                            if (drawer.selectedArrayLastIndex == 2)
                            {
                                drawer.selectedArrayLastIndex = 0;
                            }
                            newForm.PrintDebug("Selected node with index : " + i + " \n");
                            drawer.selectedNodes[drawer.selectedArrayLastIndex] = i;
                            drawer.selectedArrayLastIndex++;
                            break;
                        }
                        else
                        {
                            int?[] dest = new int?[drawer.selectedNodes.Length - 1];

                            if (a >= 0)
                            {
                                Array.Copy(drawer.selectedNodes, 0, dest, 0, a);
                            }

                            if (a < drawer.selectedNodes.Length - 1)
                            {
                                Array.Copy(drawer.selectedNodes, a + 1, dest, a, drawer.selectedNodes.Length - a - 1);
                            }

                            drawer.selectedNodes = dest;
                            if (drawer.selectedArrayLastIndex > 0)
                            {
                                drawer.selectedArrayLastIndex--;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                newForm.PrintDebug("ERROR: " + ex.ToString() + "\n");
            }
        }

        //key down event handler
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                drawer.createLines();
            }
            else if (e.KeyData == Keys.Delete)
            {
                drawer.deleteLines();
            }
            else if (e.KeyData == Keys.R)
            {
                resetScreen();
                if (comPort.IsOpen)
                {
                    comPort.Write("s"); //swipe
                }
            }
            else if (e.KeyData == (Keys.Control | Keys.R))
            {
                resetNodes();
                resetScreen();
            }
            else if (e.KeyData == Keys.Right)
            {

                leftKeyIsPressed = false;
                rightKeyIsPressed = true;
            }
            else if (e.KeyData == Keys.Left)
            {

                rightKeyIsPressed = false;
                leftKeyIsPressed = true;
            }
            else if (e.KeyData == Keys.Up)
            {
                upKeyIsPressed = true;
                downKeyIsPressed = false;

            }
            else if (e.KeyData == Keys.Down)
            {
                upKeyIsPressed = false;
                downKeyIsPressed = true;
            }
            else if (e.KeyData == (Keys.Control | Keys.Right))
            {
                ctrlLeftKeyIsPressed = false;
                ctrlRightKeyIsPressed = true;
            }
            else if (e.KeyData == (Keys.Control | Keys.Left))
            {
                ctrlLeftKeyIsPressed = true;
                ctrlRightKeyIsPressed = false;
            }
            else if (e.KeyData == Keys.NumPad6)
            {
                rotationBottomRight[0] = 90;
                refreshScreen();
            }
            else if (e.KeyData == Keys.NumPad4)
            {
                rotationBottomRight[0] = 270;
                refreshScreen();
            }
            else if (e.KeyData == Keys.NumPad8)
            {
                rotationBottomRight[0] = 0;
                refreshScreen();
            }
            else if (e.KeyData == Keys.NumPad2)
            {
                rotationBottomRight[0] = 180;
                refreshScreen();
            }
        }

        //key up event handler
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Right)
            {

                leftKeyIsPressed = false;
                rightKeyIsPressed = false;
            }
            else if (e.KeyData == Keys.Left)
            {

                rightKeyIsPressed = false;
                leftKeyIsPressed = false;
            }
            else if (e.KeyData == Keys.Up)
            {

                upKeyIsPressed = false;
                downKeyIsPressed = false;
            }
            else if (e.KeyData == Keys.Down)
            {


                upKeyIsPressed = false;
                downKeyIsPressed = false;
            }
            else if (e.KeyData == (Keys.Control | Keys.Right))
            {
                ctrlLeftKeyIsPressed = false;
                ctrlRightKeyIsPressed = false;
            }
            else if (e.KeyData == (Keys.Control | Keys.Left))
            {
                ctrlLeftKeyIsPressed = false;
                ctrlRightKeyIsPressed = false;
            }
        }

        //button to open window where you can select a file and then reads it
        private void OpenFile_Click(object sender, EventArgs e)
        {
            resetNodes();
            Stream myStream = null;
            StreamReader myReader = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();


            openFileDialog1.Filter = "Node3D Files (*.n3d*)|*.n3d*|Microsoft 3D Builder (*.obj*)|*.obj*|All Files (*.*)|*.*";
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
                                resetScreen();
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

        //button for opening window where you can select a location for the to be saved file
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

        //button to perform a reset on screen
        private void ResetScr_Click(object sender, EventArgs e)
        {
            resetScreen();
        }

        //button to perform a reset on all nodes and screen
        private void DeleteAllNodes_Click(object sender, EventArgs e)
        {
            resetNodes();
        }

        // button to creates line between two selected nodes
        private void DrawLine_Click(object sender, EventArgs e)
        {
            createLines();
        }

        //shows debug window
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

        //opens form where the actual upload will be performed
        private void UploadFPGA_Click(object sender, EventArgs e)
        {
            newForm.PrintDebug("button clicked");


            newForm.PrintDebug("connection with serial");

            if (comPort.IsOpen)
            {
                FpgaUpload uploadForm = new FpgaUpload(comPort, drawer.generateStringFPGA());
                uploadForm.Show();
            }
            else
            {
                FpgaUpload uploadForm = new FpgaUpload(drawer.generateStringFPGA());
                uploadForm.Show();
            }
        }

        //event method for receving serial information
        private void DataReceivedHandler(
                         object sender,
                         SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            received = true;
        }
        //button for deleting all lines.
        private void DeleteAllLines_Click(object sender, EventArgs e)
        {

            drawer.DeleteAllLines();
            resetScreen();
        }

        //opens a new form for generating lines between nodes of .obj files.
        private void GenerateLines_Click(object sender, EventArgs e)
        {
            GenerateAllLines generate = new GenerateAllLines(drawer, this);

            generate.Show();
        }
    }
}
