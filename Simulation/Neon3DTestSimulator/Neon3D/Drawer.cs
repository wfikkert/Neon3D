using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Neon3D
{
    class Drawer : Form
    {
        //constructor
        private Form1 mainForm = null;

        //setup debug callback
        public Action<string> DebugCallBack;

        //array for nodes and lines
        public float?[,,] starteEndnodes { get; set; }
        public float?[,] allNodes { get; set; }
        public int?[] selectedNodes { get; set; }

        //global trackers
        public int lineCounter { get; set; }
        public int selectedArrayLastIndex { get; set; }
        public int maxLines { get; set; }

        public bool isStillDrawing { get; set; }

        //midpoints
        public int[][] globalScreenInformation;

        //initializes forms, arrays.
        public Drawer(Form form, Action<string> callback, int maxAmountOflines, int[][] globalScreenInformation)
        {
            DebugCallBack = callback;
            mainForm = form as Form1;
            this.globalScreenInformation = globalScreenInformation;
            starteEndnodes = new float?[maxAmountOflines, 2, 3];
            allNodes = new float?[maxAmountOflines * 2, 3];
            selectedNodes = new int?[maxAmountOflines * 2];
            maxLines = maxAmountOflines;
            lineCounter = 0;
            selectedArrayLastIndex = 0;
        }
        //reads the arrays when you open a .n3d file.
        public void setArrays(string array)
        {
            /* array example */
            /*
                [0, [0,[0,-159],[1,112],[2,115]], [1,[0,78],[1,133],[2,102]]]
                [1, [0,[0,78],[1,133],[2,102]], [1,[0,222],[1,144],[2,185]]]
                [2, [0,[0,-159],[1,112],[2,115]], [1,[0,222],[1,144],[2,185]]]
            */

            if (array.Contains('v'))
            {
                DebugCallBack("FILE IS OBJ FILE\n");
                string[] stringLines = array.Split(new string[] { Environment.NewLine, "\r\n", "\n" }, StringSplitOptions.None);
                int ammountOfNodes = 0;

                int?[,] faces = new int?[1000, 100];
                foreach (string stringlines in stringLines)
                {
                    if (stringlines.Contains("v "))
                    {
                        string newStringLine = stringlines.Replace("v ", "");
                        string[] coordinates = newStringLine.Split(' ');
                        allNodes[ammountOfNodes, 0] = (int)float.Parse(coordinates[0], CultureInfo.InvariantCulture.NumberFormat);
                        allNodes[ammountOfNodes, 1] = (int)float.Parse(coordinates[1], CultureInfo.InvariantCulture.NumberFormat);
                        allNodes[ammountOfNodes, 2] = (int)float.Parse(coordinates[2], CultureInfo.InvariantCulture.NumberFormat);
                        DebugCallBack("ADDED COORDINATE (" + coordinates[0] + "," + coordinates[1] + "," + coordinates[2] + ") \n");

                        ammountOfNodes++;
                    }                                      

                }

                int ammountOfConnections = 4;

                int?[,] nodeIndexAlreadyParsed = new int?[ammountOfNodes, ammountOfConnections + 1];


                int indexOfClosestNode = 0;
                for (int i = 0; i < ammountOfNodes; i++)
                {
                    float? nodeX = allNodes[i, 0];
                    float? nodeY = allNodes[i, 1];
                    float? nodeZ = allNodes[i, 2];
                    
                    for(int c = 0; c < ammountOfConnections; c++)
                    {
                        float? currentClosestDistance = 10000;
                        for (int a = 0; a < ammountOfNodes; a++)
                        {

                           // DebugCallBack("Ammount of nodes: " + ammountOfNodes + " \n Checking node: " + (a - indexesParsed) + "\n");
                          
                            float? nodeXToCompare = allNodes[a, 0];
                            float? nodeYToCompare = allNodes[a, 1];
                            float? nodeZToCompare = allNodes[a, 2];


                            float? distanceX = 0;
                            float? distanceY = 0;
                            float? distanceZ = 0;

                            if (nodeX < nodeXToCompare)
                            {
                                distanceX = (nodeX * -1) + nodeXToCompare;
                            }
                            else
                            {
                                distanceX = nodeX + (nodeXToCompare * -1);
                            }

                            if (nodeY < nodeYToCompare)
                            {
                                distanceY = (nodeY * -1) + nodeYToCompare;
                            }
                            else
                            {
                                distanceY = nodeY + (nodeYToCompare * -1);
                            }
                            if (nodeZ < nodeZToCompare)
                            {
                                distanceZ = (nodeZ * -1) + nodeZToCompare;
                            }
                            else
                            {
                                distanceZ = nodeZ + (nodeZToCompare * -1);
                            }

                            float? tempClosestDistance = (float?)Math.Sqrt(Math.Pow((float)(nodeX - (nodeXToCompare)), 2) + Math.Pow((float)(nodeY - (nodeYToCompare)), 2) + Math.Pow((float)(nodeZ - (nodeZToCompare)), 2));
                            //  DebugCallBack("Node with index: " + a + " is has distance from node with index" + i + ": " + tempClosestDistance + "\n");


                            bool found = false;
                            for (int b = 0; b <= ammountOfConnections; b++)
                            {

                                if (nodeIndexAlreadyParsed[i, b] == a)
                                {

                                    DebugCallBack("Node with index " + a + " already is used for the current node with index " + i + " \n");
                                    found = true;
                                    break;
                                }
                                else
                                {
                                    found = false;
                                }
                            }

                            if (currentClosestDistance > tempClosestDistance && tempClosestDistance > 0)
                            {

                               
                                if (!found)
                                {
                                    currentClosestDistance = tempClosestDistance;
                                    indexOfClosestNode = a;
                                }

                            }
                        }


                        DebugCallBack("Node with index: " + indexOfClosestNode + " is closest node to current node with index " + i + "\n");
                        nodeIndexAlreadyParsed[i, c] = indexOfClosestNode;
                    }
                    
                }

                for(int i = 0; i < ammountOfNodes; i++)
                {
                    DebugCallBack("iterating over node with index " + i + " \n");
                    for (int b = 0; b <= ammountOfConnections; b++)
                    {

                        DebugCallBack("iterating over start node with index " + i + " and end node with index + " + nodeIndexAlreadyParsed[i, b] + "\n");
                        if(nodeIndexAlreadyParsed[i, b] == null)
                        {
                            
                        } else
                        {
                            float startNodeX = (float)allNodes[i, 0];
                            float startNodeY = (float)allNodes[i, 1];
                            float startNodeZ = (float)allNodes[i, 2];
                            float endNodeX = (float)allNodes[(int)nodeIndexAlreadyParsed[i, b], 0];
                            float endNodeY = (float)allNodes[(int)nodeIndexAlreadyParsed[i, b], 1];
                            float endNodeZ = (float)allNodes[(int)nodeIndexAlreadyParsed[i, b], 2];

                            DebugCallBack("LINE OF FACE (" + startNodeX + "," + startNodeY + "," + startNodeZ + " | " + endNodeX + ", " + endNodeY + "," + endNodeZ + ") \n");
                            bool found = false;
                            for (int a = 0; a < maxLines; a++)
                            {
                                if (starteEndnodes[a, 0, 0] == startNodeX && starteEndnodes[a, 0, 1] == startNodeY && starteEndnodes[a, 0, 2] == startNodeZ && starteEndnodes[a, 1, 0] == endNodeX && starteEndnodes[a, 1, 1] == endNodeY && starteEndnodes[a, 1, 2] == endNodeZ)
                                {
                                    DebugCallBack("LINE ALREADY EXISTS \n");
                                    found = true;
                                    break;
                                }
                                else if (starteEndnodes[a, 0, 0] == endNodeX && starteEndnodes[a, 0, 1] == endNodeY && starteEndnodes[a, 0, 2] == endNodeZ && starteEndnodes[a, 1, 0] == startNodeX && starteEndnodes[a, 1, 1] == startNodeY && starteEndnodes[a, 1, 2] == startNodeZ)
                                {
                                    DebugCallBack("LINE ALREADY EXISTS \n");
                                    found = true;
                                    break;
                                }
                                else if (starteEndnodes[a, 0, 0] == null)
                                {
                                    break;
                                }
                            }

                            if (!found)
                            {
                                DebugCallBack("ADDING LINE\n");
                                starteEndnodes[lineCounter, 0, 0] = startNodeX;
                                starteEndnodes[lineCounter, 0, 1] = startNodeY;
                                starteEndnodes[lineCounter, 0, 2] = startNodeZ;
                                starteEndnodes[lineCounter, 1, 0] = endNodeX;
                                starteEndnodes[lineCounter, 1, 1] = endNodeY;
                                starteEndnodes[lineCounter, 1, 2] = endNodeZ;
                                lineCounter++;
                            }
                        }
                       
                    }

                }





                /*
                foreach(string stringlines in stringLines)
                {
                    if (stringlines.Contains("f "))
                    {
                        string newStringLine = stringlines.Replace("f ", "");
                        string[] nodes = newStringLine.Split(' ');
                        for (int i = 0; i < nodes.Length; i++)
                        {
                            string node;
                            if (nodes[i].Contains('/'))
                            {
                                node = nodes[i].Split('/')[0];
                            }
                            else
                            {
                                node = nodes[i];
                            }
                            int currentNode = int.Parse(nodes[i]) - 1;
                            int nextNode = 0;
                            try
                            {
                                nextNode = int.Parse(nodes[i + 1]) - 1;
                            }
                            catch (Exception e)
                            {
                                nextNode = 0;
                            }

                            float startNodeX = (float)allNodes[currentNode, 0];
                            float startNodeY = (float)allNodes[currentNode, 1];
                            float startNodeZ = (float)allNodes[currentNode, 2];
                            float endNodeX = (float)allNodes[nextNode, 0];
                            float endNodeY = (float)allNodes[nextNode, 1];
                            float endNodeZ = (float)allNodes[nextNode, 2];

                            DebugCallBack("LINE OF FACE (" + startNodeX + "," + startNodeY + "," + startNodeZ + " | " + endNodeX + ", " + endNodeY + "," + endNodeZ + ") \n");
                            bool found = false;
                            for (int a = 0; a < maxLines; a++)
                            {
                                if (starteEndnodes[a, 0, 0] == startNodeX && starteEndnodes[a, 0, 1] == startNodeY && starteEndnodes[a, 0, 2] == startNodeZ && starteEndnodes[a, 1, 0] == endNodeX && starteEndnodes[a, 1, 1] == endNodeY && starteEndnodes[a, 1, 2] == endNodeZ)
                                {
                                    DebugCallBack("LINE ALREADY EXISTS \n");
                                    found = true;
                                    break;
                                }
                                else if (starteEndnodes[a, 0, 0] == endNodeX && starteEndnodes[a, 0, 1] == endNodeY && starteEndnodes[a, 0, 2] == endNodeZ && starteEndnodes[a, 1, 0] == startNodeX && starteEndnodes[a, 1, 1] == startNodeY && starteEndnodes[a, 1, 2] == startNodeZ)
                                {
                                    DebugCallBack("LINE ALREADY EXISTS \n");
                                    found = true;
                                    break;
                                }
                                else if (starteEndnodes[a, 0, 0] == null)
                                {
                                    break;
                                }
                            }

                            if (!found)
                            {
                                DebugCallBack("ADDING LINE\n");
                                starteEndnodes[lineCounter, 0, 0] = startNodeX;
                                starteEndnodes[lineCounter, 0, 1] = startNodeY;
                                starteEndnodes[lineCounter, 0, 2] = startNodeZ;
                                starteEndnodes[lineCounter, 1, 0] = endNodeX;
                                starteEndnodes[lineCounter, 1, 1] = endNodeY;
                                starteEndnodes[lineCounter, 1, 2] = endNodeZ;
                                lineCounter++;
                            }
                        }

                    }
                }*/
            } else
            {
                string[] stringLines = array.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                DebugCallBack("first index of stringLines: " + stringLines[0] + " \n");
                lineCounter = 0;
                string stringline = stringLines[0];
                if (stringline != "" || stringline != null)
                {
                    DebugCallBack("stringline: " + stringline + " \n");
                    try
                    {
                        string[] parts = stringline.Split('[');
                        //int lineIndex = Int32.Parse(parts[1].Split(',')[0]);
                        float startNodeX = float.Parse(parts[3].Split(',')[1].Split(']')[0]);
                        float startNodeY = float.Parse(parts[4].Split(',')[1].Split(']')[0]);
                        float startNodeZ = float.Parse(parts[5].Split(',')[1].Split(']')[0]);
                        float endNodeX = float.Parse(parts[7].Split(',')[1].Split(']')[0]);
                        float endNodeY = float.Parse(parts[8].Split(',')[1].Split(']')[0]);
                        float endNodeZ = float.Parse(parts[9].Split(',')[1].Split(']')[0]);
                        starteEndnodes[lineCounter, 0, 0] = startNodeX;
                        starteEndnodes[lineCounter, 0, 1] = startNodeY;
                        starteEndnodes[lineCounter, 0, 2] = startNodeZ;
                        starteEndnodes[lineCounter, 1, 0] = endNodeX;
                        starteEndnodes[lineCounter, 1, 1] = endNodeY;
                        starteEndnodes[lineCounter, 1, 2] = endNodeZ;
                        lineCounter++;
                        DebugCallBack("ADDED: " + stringline + " \n");
                        bool added = false;
                        for (int i = 0; i < (allNodes.Length); i++)
                        {
                            if (allNodes[i, 0] == null)
                            {
                                allNodes[i, 0] = startNodeX;
                                allNodes[i, 1] = startNodeY;
                                allNodes[i, 2] = startNodeZ;
                                allNodes[i + 1, 0] = startNodeX;
                                allNodes[i + 1, 1] = startNodeY;
                                allNodes[i + 1, 2] = startNodeZ;
                                lineCounter = i;
                                added = true;
                                break;
                            }
                        }

                        if (!added)
                        {
                            DebugCallBack("Could not add node to nodelist, array reached limit! \n");
                        }

                    }
                    catch (Exception e)
                    {
                        DebugCallBack("Cannot read line and nodes! \n");
                        DebugCallBack(e.ToString() + "\n");
                    }
                }


                foreach (string stringlines in stringLines)
                {
                    if (stringlines != "" || stringlines != null)
                    {
                        DebugCallBack("stringline: " + stringlines + " \n");
                        try
                        {
                            string[] parts = stringlines.Split('[');
                            //int lineIndex = Int32.Parse(parts[1].Split(',')[0]);
                            float startNodeX = float.Parse(parts[3].Split(',')[1].Split(']')[0]);
                            float startNodeY = float.Parse(parts[4].Split(',')[1].Split(']')[0]);
                            float startNodeZ = float.Parse(parts[5].Split(',')[1].Split(']')[0]);
                            float endNodeX = float.Parse(parts[7].Split(',')[1].Split(']')[0]);
                            float endNodeY = float.Parse(parts[8].Split(',')[1].Split(']')[0]);
                            float endNodeZ = float.Parse(parts[9].Split(',')[1].Split(']')[0]);
                            starteEndnodes[lineCounter, 0, 0] = startNodeX;
                            starteEndnodes[lineCounter, 0, 1] = startNodeY;
                            starteEndnodes[lineCounter, 0, 2] = startNodeZ;
                            starteEndnodes[lineCounter, 1, 0] = endNodeX;
                            starteEndnodes[lineCounter, 1, 1] = endNodeY;
                            starteEndnodes[lineCounter, 1, 2] = endNodeZ;
                            lineCounter++;
                            DebugCallBack("ADDED: " + stringlines + " \n");
                            bool added = false;
                            for (int i = 0; i < (allNodes.Length); i++)
                            {
                                if (allNodes[i, 0] == null)
                                {
                                    allNodes[i, 0] = startNodeX;
                                    allNodes[i, 1] = startNodeY;
                                    allNodes[i, 2] = startNodeZ;
                                    allNodes[i + 1, 0] = startNodeX;
                                    allNodes[i + 1, 1] = startNodeY;
                                    allNodes[i + 1, 2] = startNodeZ;
                                    lineCounter = i;
                                    added = true;
                                    break;
                                }
                            }

                            if (!added)
                            {
                                DebugCallBack("Could not add node to nodelist, array reached limit! \n");
                            }

                        }
                        catch (Exception e)
                        {
                            DebugCallBack("Cannot read line and nodes! \n");
                            DebugCallBack(e.ToString() + "\n");
                        }
                    }
                }
            }

           
        }
        //generate a string to send the object to the FPGA, if it is below 0 then it sends a m if it is higher than 0 it sends a p after the coordinate is sent.
        public string generateStringFPGA()
        {
            /*
            array example
            {0,100,0,100,0,100,0,100,0,100,0,-100,0,100,0,-100,0,100,0,100,0,-100,0,-100,-100,0,-100,-100,0,100,100,0,-100,100,0,100,-100,0,-100,100,0,-100,-100,0,100,100,0,100};
            */
            int bound0 = starteEndnodes.GetUpperBound(0);
            int bound1 = starteEndnodes.GetUpperBound(1);
            int bound2 = starteEndnodes.GetUpperBound(2);

            StringBuilder buildAstring = new StringBuilder();
            string valuestr = "";

            StringBuilder buildAstring2 = new StringBuilder();
            string valuestr2 = "";
            buildAstring2.Append("{");
            for (int variable0 = 0; variable0 <= bound0; variable0++)
            {
                if (starteEndnodes[variable0, 0, 0] != null)
                {
                    for (int variable1 = 0; variable1 <= bound1; variable1++)
                    {

                        for (int variable2 = 0; variable2 <= bound2; variable2++)
                        {
                            float? value = starteEndnodes[variable0, variable1, variable2];
                            if (value >= 0)
                            {

                                valuestr = value + "p";
                                valuestr2 = value.ToString() + " , ";
                            }
                            else
                            {
                                valuestr2 =  value.ToString() + " , ";
                                value = value * -1;
                                valuestr = value + "m";
                                
                            }
                            buildAstring.Append(valuestr);
                            buildAstring2.Append(valuestr2);

                        }
                    }
                }
            }
            buildAstring.Append(Environment.NewLine);

            string fpgaArray = buildAstring2.ToString();
            string fpgaArray2 = fpgaArray.Substring(0, fpgaArray.Length - 1) + "}";
            return buildAstring.ToString() + "~" + fpgaArray2;
        }
        //generates a string to save the object in a .n3d file.
        public string generateString()
        {
            /* array example */
            /*
                [0, [0, [0, "x"],[1,"y"],[2,"z"]] , [1,[0, "x"],[1,"y"],[2,"z"]]]  *line 0
                [1, [0, [0, "x"],[1,"y"],[2,"z"]] , [1,[0, "x"],[1,"y"],[2,"z"]]]  *line 1
            */
            int bound0 = starteEndnodes.GetUpperBound(0);
            int bound1 = starteEndnodes.GetUpperBound(1);
            int bound2 = starteEndnodes.GetUpperBound(2);

            StringBuilder buildAstring = new StringBuilder();
            for (int variable0 = 0; variable0 <= bound0; variable0++)
            {
                if (starteEndnodes[variable0, 0, 0] != null)
                {
                    buildAstring.Append("[" + variable0.ToString());
                    for (int variable1 = 0; variable1 <= bound1; variable1++)
                    {
                        buildAstring.Append(", [" + variable1.ToString());
                        for (int variable2 = 0; variable2 <= bound2; variable2++)
                        {
                            float? value = starteEndnodes[variable0, variable1, variable2];
                            buildAstring.Append(",[" + variable2.ToString() + "," + value + "]");
                        }
                        buildAstring.Append("]");
                    }
                    buildAstring.Append("]" + Environment.NewLine);
                }
            }
            return buildAstring.ToString();
        }

        //draws a pixel at given location
        public void drawPixel(int x, int y, int size, int redValue, int greenValue, int blueValue)
        {
            Color myColor = Color.FromArgb(redValue, greenValue, blueValue);
            SolidBrush brushColor = new SolidBrush(myColor);
            try
            {
                Graphics g = mainForm.CreateGraphics();
                if (x != 0 && y != 0)
                {
                    g.FillRectangle(brushColor, x, y, size, size);
                    g.Dispose();
                    brushColor.Dispose();
                }
            }
            catch (Exception e)
            {
                DebugCallBack("EXCEPTION WHILE DRAWING PIXEL:\n " + e.ToString() + "\n");
            }
        }

        //removes a pixel at given location
        public void removePixel(int x, int y, int size)
        {

            Color myColor = Color.FromArgb(255, 255, 255);
            SolidBrush brushColor = new SolidBrush(myColor);
            try
            {
                Graphics g = mainForm.CreateGraphics();
                if (x != 0 && y != 0)
                {
                    g.FillRectangle(brushColor, x, y, size, size);
                    g.Dispose();
                    brushColor.Dispose();
                }
            }
            catch (Exception e)
            {
                DebugCallBack("EXCEPTION WHILE REMOVING PIXEL:\n " + e.ToString() + "\n");
            }

        }

        //ques the drawline logic in a threadpool, which tries to runs everything in the pool simultainously
        public void drawLine(float x1, float y1, float x2, float y2, float midX, float midY, int redValue, int greenValue, int blueValue, int size, bool remove)
        {
            ThreadPool.QueueUserWorkItem(drawLineQue, new object[] { x1, y1, x2, y2, midX, midY, redValue, greenValue, blueValue, size, remove });
        }

        //in between method needed to que the line for the threads
        public void drawLineQue(object param)
        {
            object[] parameters = param as object[];
            float x1 = float.Parse(parameters[0].ToString());
            float y1 = float.Parse(parameters[1].ToString());
            float x2 = float.Parse(parameters[2].ToString());
            float y2 = float.Parse(parameters[3].ToString());
            float midx = float.Parse(parameters[4].ToString());
            float midy = float.Parse(parameters[5].ToString());
            int redValue = Int32.Parse(parameters[6].ToString());
            int greenValue = Int32.Parse(parameters[7].ToString());
            int blueValue = Int32.Parse(parameters[8].ToString());
            int size = Int32.Parse(parameters[9].ToString());
            bool remove = Convert.ToBoolean(parameters[10].ToString());
            drawLineLogic(x1, y1, x2, y2, midx, midy, redValue, greenValue, blueValue, size, remove);
        }

        //draws a line between two given points, related to the mid point of the screen
        public void drawLineLogic(float x1, float y1, float x2, float y2, float midX, float midY, int redValue, int greenValue, int blueValue, int size, bool remove)
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
                float tempX = x1;
                float tempY = y1;
                x1 = x2;
                y1 = y2;

                x2 = tempX;
                y2 = tempY;
            }

            //setup vars for loop
            float x;
            float prevY = 0;
            float prevX = 0;
            //for loop for drawing beam
            for (x = x1; x <= x2; x++)
            {
                float y = 0;
                try
                {
                    //formula for drawing beam between nodes
                    if ((x2 - x1) == 0)
                    {
                        float count;

                        if (y2 > y1)
                        {
                            float tempY = y1;
                            y1 = y2;
                            y2 = tempY;
                        }
                        for (count = y2; count <= y1; count++)
                        {
                            if (!remove)
                            {
                                drawPixel((int)x, (int)count, size, redValue, greenValue, blueValue);
                            }
                            else
                            {
                                removePixel((int)x, (int)count, size);
                            }
                        }
                    }
                    else
                    {
                        y = ((y2 - y1) / (x2 - x1)) * (x - x1) + y1;
                    }
                }
                catch (Exception e)
                {
                    DebugCallBack("Error: " + e.ToString() + " \n");
                    y = globalScreenInformation[0][1];
                }

                //drawing all pixels of beam between nodes
                if (y - prevY > 0 && x != 0)
                {
                    float counter;
                    for (counter = prevY; counter <= y; counter++)
                    {
                        if (!remove)
                        {
                            drawPixel((int)prevX, (int)counter, size, redValue, greenValue, blueValue);
                        }
                        else
                        {
                            removePixel((int)prevX, (int)counter, size);
                        }
                    }
                }
                else if (prevY - y > 0 && x != 0)
                {
                    float counter;
                    for (counter = y; counter <= prevY; counter++)
                    {
                        if (!remove)
                        {
                            drawPixel((int)prevX, (int)counter, size, redValue, greenValue, blueValue);
                        }
                        else
                        {
                            removePixel((int)prevX, (int)counter, size);
                        }
                    }
                }

                //draw line/remove prev line
                if (!remove)
                {
                    drawPixel((int)x, (int)y, size, redValue, greenValue, blueValue);
                }
                else
                {
                    removePixel((int)x, (int)y, size);
                }
                prevY = y;
                prevX = x;
            }
        }

        //draws a z and y axle in a view 
        public void drawAxMatrix(int screen, int r, int g, int b, int size)
        {
            drawLine(globalScreenInformation[screen][0], 0, -globalScreenInformation[screen][0], 0, globalScreenInformation[screen][0], globalScreenInformation[screen][1], r, g, b, size, false);
            drawLine(0, globalScreenInformation[screen][1], 0, -globalScreenInformation[screen][1], globalScreenInformation[screen][0], globalScreenInformation[screen][1], r, g, b, size, false);
        }

        public void deleteLines()
        {
            
           
                if (selectedNodes[0] != null && selectedNodes[1] != null && selectedNodes[0] != selectedNodes[1])
                {


                    float startNodeX = (float)allNodes[(int)selectedNodes[0].Value, 0];
                    float startNodeY = (float)allNodes[(int)selectedNodes[0].Value, 1];
                    float startNodeZ = (float)allNodes[(int)selectedNodes[0].Value, 2];
                    float endNodeX = (float)allNodes[(int)selectedNodes[1].Value, 0];
                    float endNodeY = (float)allNodes[(int)selectedNodes[1].Value, 1];
                    float endNodeZ = (float)allNodes[(int)selectedNodes[1].Value, 2];

                    DebugCallBack("LINE OF FACE (" + startNodeX + "," + startNodeY + "," + startNodeZ + " | " + endNodeX + ", " + endNodeY + "," + endNodeZ + ") \n");
                    bool found = false;
                    for (int a = 0; a < maxLines; a++)
                    {
                        if (starteEndnodes[a, 0, 0] == startNodeX && starteEndnodes[a, 0, 1] == startNodeY && starteEndnodes[a, 0, 2] == startNodeZ && starteEndnodes[a, 1, 0] == endNodeX && starteEndnodes[a, 1, 1] == endNodeY && starteEndnodes[a, 1, 2] == endNodeZ)
                        {
                            DebugCallBack("LINE EXISTS \n");
                            starteEndnodes[a, 0, 0] = null;
                            starteEndnodes[a, 0, 1] = null;
                            starteEndnodes[a, 0, 2] = null;
                            starteEndnodes[a, 1, 0] = null;
                            starteEndnodes[a, 1, 1] = null;
                            starteEndnodes[a, 1, 2] = null;
                        }
                        else if (starteEndnodes[a, 0, 0] == endNodeX && starteEndnodes[a, 0, 1] == endNodeY && starteEndnodes[a, 0, 2] == endNodeZ && starteEndnodes[a, 1, 0] == startNodeX && starteEndnodes[a, 1, 1] == startNodeY && starteEndnodes[a, 1, 2] == startNodeZ)
                        {
                            DebugCallBack("LINE EXISTS \n");
                            starteEndnodes[a, 0, 0] = null;
                            starteEndnodes[a, 0, 1] = null;
                            starteEndnodes[a, 0, 2] = null;
                            starteEndnodes[a, 1, 0] = null;
                            starteEndnodes[a, 1, 1] = null;
                            starteEndnodes[a, 1, 2] = null;
                        }
                        else if (starteEndnodes[a, 0, 0] == null)
                        {
                            DebugCallBack("Line doesn't exist \n");
                        }
                    }

                    mainForm.resetScreen();
                    mainForm.resetScreen();
                    selectedNodes = new int?[maxLines * 2];
                    selectedArrayLastIndex = 0;
                }
                else
                {
                    MessageBox.Show("You need to select 2 nodes!!!", "Hint");
                }
               
            
        }
        

        //creates a line between two nodes
        public void createLines()
        {
           
            if (selectedNodes[0] != null && selectedNodes[1] != null)
            {
                        
                float startNodeX = (float)allNodes[(int)selectedNodes[0], 0];
                float startNodeY = (float)allNodes[(int)selectedNodes[0], 1];
                float startNodeZ = (float)allNodes[(int)selectedNodes[0], 2];
                float endNodeX = (float)allNodes[(int)selectedNodes[1], 0];
                float endNodeY = (float)allNodes[(int)selectedNodes[1], 1];
                float endNodeZ = (float)allNodes[(int)selectedNodes[1], 2];

                DebugCallBack("LINE OF FACE (" + startNodeX + "," + startNodeY + "," + startNodeZ + " | " + endNodeX + ", " + endNodeY + "," + endNodeZ + ") \n");
                bool found = false;
                for (int a = 0; a < maxLines; a++)
                {
                    if (starteEndnodes[a, 0, 0] == startNodeX && starteEndnodes[a, 0, 1] == startNodeY && starteEndnodes[a, 0, 2] == startNodeZ && starteEndnodes[a, 1, 0] == endNodeX && starteEndnodes[a, 1, 1] == endNodeY && starteEndnodes[a, 1, 2] == endNodeZ)
                    {
                        DebugCallBack("LINE ALREADY EXISTS \n");
                        found = true;
                        break;
                    }
                    else if (starteEndnodes[a, 0, 0] == endNodeX && starteEndnodes[a, 0, 1] == endNodeY && starteEndnodes[a, 0, 2] == endNodeZ && starteEndnodes[a, 1, 0] == startNodeX && starteEndnodes[a, 1, 1] == startNodeY && starteEndnodes[a, 1, 2] == startNodeZ)
                    {
                        DebugCallBack("LINE ALREADY EXISTS \n");
                        found = true;
                        break;
                    }
                    else if (starteEndnodes[a, 0, 0] == null)
                    {
                        DebugCallBack("Line doesn't exist \n");
                        break;
                    }
                }

                if (!found)
                {
                        
                    starteEndnodes[lineCounter, 0, 0] = allNodes[selectedNodes[0].Value, 0];
                    starteEndnodes[lineCounter, 0, 1] = allNodes[selectedNodes[0].Value, 1];
                    starteEndnodes[lineCounter, 0, 2] = allNodes[selectedNodes[0].Value, 2];
                    starteEndnodes[lineCounter, 1, 0] = allNodes[selectedNodes[1].Value, 0];
                    starteEndnodes[lineCounter, 1, 1] = allNodes[selectedNodes[1].Value, 1];
                    starteEndnodes[lineCounter, 1, 2] = allNodes[selectedNodes[1].Value, 2];
                    lineCounter++;
                        
                }else
                {
                    MessageBox.Show("Lines are already drawn between those two nodes", "Error");
                }
                        
                        

                selectedNodes = new int?[maxLines * 2];
                selectedArrayLastIndex = 0;
            }
            else
            {
                MessageBox.Show("You need to select 2 nodes!!!", "Hint");
            }
               
        }

        public float prevousZoomScreenTL = 0;
        public float prevousZoomScreenTR = 0;
        public float prevousZoomScreenBR = 0;
        public float prevousZoomScreenBL = 0;
        public float previousXRotation = 0;
        public float previousYRotation = 0;
        public float previousZRotation = 0;

        //draws the nodes and beams on screen for every view
        public void drawNodesAndBeams(int conv3dto2d, int screen, float zoomscreen, int[] rotation)
        {
            //isStillDrawing = true;
            int linesDrawn;
            float startx = 0;
            float starty = 0;
            float startz = 0;
            float endx = 0;
            float endy = 0;
            float endz = 0;



            for (int i = 0; i < (allNodes.Length / 3); i++)
            {
                if (allNodes[i, 0] != null && allNodes[i, 1] != null)
                {
                    startx = allNodes[i, 0].Value;
                    starty = allNodes[i, 1].Value;
                    if (conv3dto2d == 0)
                    {
                        starty = allNodes[i, 2].Value;
                    }
                    else if (conv3dto2d == 1)
                    {
                        startx = -allNodes[i, 0].Value;
                        starty = allNodes[i, 2].Value;
                    }
                    else if (conv3dto2d == 3)
                    {
                        startx = -allNodes[i, 0].Value;
                    }
                    else if (conv3dto2d == 4)
                    {
                        startx = allNodes[i, 2].Value;
                    }
                    else if (conv3dto2d == 5)
                    {
                        startx = -allNodes[i, 2].Value;
                    }


                    bool found = false;
                    for (int selectedcounter = 0; selectedcounter < selectedNodes.Length; selectedcounter++)
                    {
                        switch (screen)
                        {
                            case 1:
                                if (zoomscreen != prevousZoomScreenTL)
                                {
                                    removePixel((int)(((startx * prevousZoomScreenTL) + globalScreenInformation[screen][0])), (int)((globalScreenInformation[screen][1] - (starty * prevousZoomScreenTL))), 4);
                                }
                                break;
                            case 2:
                                if (zoomscreen != prevousZoomScreenTR)
                                {
                                    removePixel((int)(((startx * prevousZoomScreenTR) + globalScreenInformation[screen][0])), (int)((globalScreenInformation[screen][1] - (starty * prevousZoomScreenTR))), 4);
                                }
                                break;
                            case 3:
                                if (zoomscreen != prevousZoomScreenBL)
                                {
                                    removePixel((int)(((startx * prevousZoomScreenBL) + globalScreenInformation[screen][0])), (int)((globalScreenInformation[screen][1] - (starty * prevousZoomScreenBL))), 4);
                                }
                                break;
                        }


                        if (conv3dto2d != 6)
                        {
                            if (selectedNodes[selectedcounter] != null)
                            {
                                DebugCallBack("there is a value in index: " + selectedcounter + ", current node index = " + i + " \n");
                                if (selectedNodes[selectedcounter] == i)
                                {
                                    DebugCallBack("value is same as node\n");
                                    drawPixel((int)(((startx * zoomscreen) + globalScreenInformation[screen][0])), (int)((globalScreenInformation[screen][1] - (starty * zoomscreen))), 4, 0, 255, 0);
                                    found = true;
                                   
                                }
                            }
                            else
                            {
                                // DebugCallBack("there is not a value in index: " + selectedcounter + " \n");
                                if (!found)
                                {
                                    drawPixel((int)(((startx * zoomscreen) + globalScreenInformation[screen][0])), (int)((globalScreenInformation[screen][1] - (starty * zoomscreen))), 4, 255, 0, 0);
                                } else
                                {
                                    drawPixel((int)(((startx * zoomscreen) + globalScreenInformation[screen][0])), (int)((globalScreenInformation[screen][1] - (starty * zoomscreen))), 4, 0, 255, 0);
                                }

                              
                            }
                        }

                      
                    } 
                }
            }


            // isStillDrawing = false;

            for (linesDrawn = 0; linesDrawn < maxLines; linesDrawn++)
            {


                if (starteEndnodes[linesDrawn, 0, 0] != null && starteEndnodes[linesDrawn, 0, 1] != null && starteEndnodes[linesDrawn, 1, 0] != null && starteEndnodes[linesDrawn, 1, 1] != null)
                {
                    float x1 = startx = starteEndnodes[linesDrawn, 0, 0].Value;
                    float y1 = starty = starteEndnodes[linesDrawn, 0, 1].Value;
                    float z1 = startz = starteEndnodes[linesDrawn, 0, 2].Value;

                    float x2 = endx = starteEndnodes[linesDrawn, 1, 0].Value;
                    float y2 = endy = starteEndnodes[linesDrawn, 1, 1].Value;
                    float z2 = endz = starteEndnodes[linesDrawn, 1, 2].Value;


                    int? node1_selected = selectedNodes[0];
                    int? node2_selected = selectedNodes[0];

                    if (conv3dto2d == 0)
                    {
                        x1 = startx;
                        x2 = endx;
                        y1 = startz;
                        y2 = endz;
                    }
                    else if (conv3dto2d == 1)
                    {
                        x1 = -startx;
                        x2 = -endx;
                        y1 = startz;
                        y2 = endz;
                    }
                    else if (conv3dto2d == 2)
                    {
                        x1 = startx;
                        x2 = endx;
                        y1 = starty;
                        y2 = endy;
                    }
                    else if (conv3dto2d == 3)
                    {
                        float rotationdifferencestart = (((startx * -1) - (startz * -1)) / 90);
                        float rotationdifferenceend = (((endx * -1) - (endz * -1)) / 90);
                        startx = -startx;
                        endx = -endx;
                    }
                    else if (conv3dto2d == 4)
                    {
                        x1 = startz;
                        x2 = endz;
                    }
                    else if (conv3dto2d == 5)
                    {
                        x1 = -startz;
                        x2 = -endz;
                    }
                    else if (conv3dto2d == 6)
                    {
                        if (previousXRotation != rotation[0] || previousYRotation != rotation[1] || previousZRotation != rotation[2])
                        {
                            x1 = (float)(((startx * Math.Cos(previousXRotation / 57.4) - (starty * Math.Cos(previousZRotation / 57.4) - startz * Math.Sin(previousZRotation / 57.4)) * Math.Sin(previousXRotation / 57.4)) * (Math.Cos(previousYRotation / 57.4))) + (starty * Math.Sin(previousZRotation / 57.4) + startz * Math.Cos(previousZRotation / 57.4)) * Math.Sin(previousYRotation / 57.4));
                            x2 = (float)(((endx * Math.Cos(previousXRotation / 57.4) - (endy * Math.Cos(previousZRotation / 57.4) - endz * Math.Sin(previousZRotation / 57.4)) * Math.Sin(previousXRotation / 57.4)) * (Math.Cos(previousYRotation / 57.4))) + (endy * Math.Sin(previousZRotation / 57.4) + endz * Math.Cos(previousZRotation / 57.4)) * Math.Sin(previousYRotation / 57.4));

                            y1 = (float)((-(startx * Math.Cos(previousXRotation / 57.4) - (starty * Math.Cos(previousZRotation / 57.4) - startz * Math.Sin(previousZRotation / 57.4)) * Math.Sin(previousXRotation / 57.4))) * (Math.Sin(previousYRotation / 57.4)) + (starty * Math.Sin(previousZRotation / 57.4) + startz * Math.Cos(previousZRotation / 57.4)) * Math.Cos(previousYRotation / 57.4));
                            y2 = (float)((-(endx * Math.Cos(previousXRotation / 57.4) - (endy * Math.Cos(previousZRotation / 57.4) - endz * Math.Sin(previousZRotation / 57.4)) * Math.Sin(previousXRotation / 57.4))) * (Math.Sin(previousYRotation / 57.4)) + (endy * Math.Sin(previousZRotation / 57.4) + endz * Math.Cos(previousZRotation / 57.4)) * Math.Cos(previousYRotation / 57.4));

                            drawLine((x1 * prevousZoomScreenBR), (y1 * prevousZoomScreenBR), (x2 * prevousZoomScreenBR), (y2 * prevousZoomScreenBR), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 255, 255, 255, 1, false);
                        }

                        x1 = (float)(((startx * Math.Cos(rotation[0] / 57.4) - (starty * Math.Cos(rotation[2] / 57.4) - startz * Math.Sin(rotation[2] / 57.4)) * Math.Sin(rotation[0] / 57.4)) * (Math.Cos(rotation[1] / 57.4))) + (starty * Math.Sin(rotation[2] / 57.4) + startz * Math.Cos(rotation[2] / 57.4)) * Math.Sin(rotation[1] / 57.4));
                        x2 = (float)(((endx * Math.Cos(rotation[0] / 57.4) - (endy * Math.Cos(rotation[2] / 57.4) - endz * Math.Sin(rotation[2] / 57.4)) * Math.Sin(rotation[0] / 57.4)) * (Math.Cos(rotation[1] / 57.4))) + (endy * Math.Sin(rotation[2] / 57.4) + endz * Math.Cos(rotation[2] / 57.4)) * Math.Sin(rotation[1] / 57.4));

                        y1 = (float)((-(startx * Math.Cos(rotation[0] / 57.4) - (starty * Math.Cos(rotation[2] / 57.4) - startz * Math.Sin(rotation[2] / 57.4)) * Math.Sin(rotation[0] / 57.4))) * (Math.Sin(rotation[1] / 57.4)) + (starty * Math.Sin(rotation[2] / 57.4) + startz * Math.Cos(rotation[2] / 57.4)) * Math.Cos(rotation[1] / 57.4));
                        y2 = (float)((-(endx * Math.Cos(rotation[0] / 57.4) - (endy * Math.Cos(rotation[2] / 57.4) - endz * Math.Sin(rotation[2] / 57.4)) * Math.Sin(rotation[0] / 57.4))) * (Math.Sin(rotation[1] / 57.4)) + (endy * Math.Sin(rotation[2] / 57.4) + endz * Math.Cos(rotation[2] / 57.4)) * Math.Cos(rotation[1] / 57.4));
                    }

                   

                    switch (screen)
                    {
                        case 1:
                            if (zoomscreen != prevousZoomScreenTL)
                            {
                                drawLine((x1 * prevousZoomScreenTL), (y1 * prevousZoomScreenTL), (x2 * prevousZoomScreenTL), (y2 * prevousZoomScreenTL), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 255, 255, 255, 1, true);
                                drawLine((x1 * zoomscreen), (y1 * zoomscreen), (x2 * zoomscreen), (y2 * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);

                            }
                            else
                            {
                                drawLine((x1 * zoomscreen), (y1 * zoomscreen), (x2 * zoomscreen), (y2 * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);
                            }
                            break;
                        case 2:
                            if (zoomscreen != prevousZoomScreenTR)
                            {
                                drawLine((x1 * prevousZoomScreenTR), (y1 * prevousZoomScreenTR), (x2 * prevousZoomScreenTR), (y2 * prevousZoomScreenTR), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 255, 255, 255, 1, true);
                                drawLine((x1 * zoomscreen), (y1 * zoomscreen), (x2 * zoomscreen), (y2 * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);
                            }
                            else
                            {
                                drawLine((x1 * zoomscreen), (y1 * zoomscreen), (x2 * zoomscreen), (y2 * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);
                            }
                            break;
                        case 3:
                            if (zoomscreen != prevousZoomScreenBL)
                            {
                                drawLine((x1 * prevousZoomScreenBL), (y1 * prevousZoomScreenBL), (x2 * prevousZoomScreenBL), (y2 * prevousZoomScreenBL), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 255, 255, 255, 1, true);
                                drawLine((x1 * zoomscreen), (y1 * zoomscreen), (x2 * zoomscreen), (y2 * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);
                            }
                            else
                            {
                                drawLine((x1 * zoomscreen), (y1 * zoomscreen), (x2 * zoomscreen), (y2 * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);
                            }

                            break;
                        case 4:
                            if (zoomscreen != prevousZoomScreenBR)
                            {
                                drawLine((x1 * prevousZoomScreenBR), (y1 * prevousZoomScreenBR), (x2 * prevousZoomScreenBR), (y2 * prevousZoomScreenBR), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 255, 255, 255, 1, true);
                                drawLine((x1 * zoomscreen), (y1 * zoomscreen), (x2 * zoomscreen), (y2 * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);
                            }
                            else
                            {
                                drawLine((x1 * zoomscreen), (y1 * zoomscreen), (x2 * zoomscreen), (y2 * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);
                            }
                            break;
                    }
                }
            

            }



            switch (screen)
            {
                case 1:
                    if (zoomscreen != prevousZoomScreenTL)
                    {
                        prevousZoomScreenTL = zoomscreen;
                    }
                    break;
                case 2:
                    if (zoomscreen != prevousZoomScreenTR)
                    {
                        prevousZoomScreenTR = zoomscreen;
                    }
                    break;
                case 3:
                    if (zoomscreen != prevousZoomScreenBL)
                    {
                        prevousZoomScreenBL = zoomscreen;
                    }
                    break;
                case 4:
                    if (zoomscreen != prevousZoomScreenBR)
                    {
                        prevousZoomScreenBR = zoomscreen;
                    }
                    previousXRotation = rotation[0];
                    previousYRotation = rotation[1];
                    previousZRotation = rotation[2];
                    break;
            }



            isStillDrawing = false;
        }
    }
}