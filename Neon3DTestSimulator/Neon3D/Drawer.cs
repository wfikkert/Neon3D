﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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
        public double?[,,] starteEndnodes { get; set; }
        public double?[,] allNodes { get; set; }
        public int?[] selectedNodes { get; set; }

        //global trackers
        public int lineCounter { get; set; }
        public int selectedArrayLastIndex { get; set; }
        public int maxLines { get; set; }

        public bool isStillDrawing { get; set; }

        //midpoints
        public int[][] globalScreenInformation;

        public Drawer(Form form, Action<string> callback, int maxAmountOflines, int[][] globalScreenInformation)
        {
            DebugCallBack = callback;
            mainForm = form as Form1;
            this.globalScreenInformation = globalScreenInformation;
            starteEndnodes = new double?[maxAmountOflines, 2, 3];
            allNodes = new double?[maxAmountOflines * 2, 3];
            selectedNodes = new int?[maxAmountOflines * 2];
            maxLines = maxAmountOflines;
            lineCounter = 0;
            selectedArrayLastIndex = 0;

        }

        public void setArrays(string array)
        {
            /* array example */
            /*
                [0, [0,[0,-159],[1,112],[2,115]], [1,[0,78],[1,133],[2,102]]]
                [1, [0,[0,78],[1,133],[2,102]], [1,[0,222],[1,144],[2,185]]]
                [2, [0,[0,-159],[1,112],[2,115]], [1,[0,222],[1,144],[2,185]]]
            */
            string[] stringLines = array.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            DebugCallBack("first index of stringLines: " + stringLines[0] + " \n");
            foreach (string stringlines in stringLines)
            {
                if(stringlines != "" || stringlines != null)
                {
                    DebugCallBack("stringline: " + stringlines + " \n");
                    try
                    {

                        string[] parts = stringlines.Split('[');
                        int lineIndex = Int32.Parse(parts[1].Split(',')[0]);
                        int startNodeX = Int32.Parse(parts[3].Split(',')[1].Split(']')[0]);
                        int startNodeY = Int32.Parse(parts[4].Split(',')[1].Split(']')[0]);
                        int startNodeZ = Int32.Parse(parts[5].Split(',')[1].Split(']')[0]);
                        int endNodeX = Int32.Parse(parts[7].Split(',')[1].Split(']')[0]);
                        int endNodeY = Int32.Parse(parts[8].Split(',')[1].Split(']')[0]);
                        int endNodeZ = Int32.Parse(parts[9].Split(',')[1].Split(']')[0]);
                        starteEndnodes[lineCounter + lineIndex, 0, 0] = startNodeX;
                        starteEndnodes[lineCounter + lineIndex, 0, 1] = startNodeY;
                        starteEndnodes[lineCounter + lineIndex, 0, 2] = startNodeZ;
                        starteEndnodes[lineCounter + lineIndex, 1, 0] = endNodeX;
                        starteEndnodes[lineCounter + lineIndex, 1, 1] = endNodeY;
                        starteEndnodes[lineCounter + lineIndex, 1, 2] = endNodeZ;

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
                if(starteEndnodes[variable0, 0, 0] != null)
                {
                    buildAstring.Append("[" + variable0.ToString());
                    for (int variable1 = 0; variable1 <= bound1; variable1++)
                    {
                        buildAstring.Append(", [" + variable1.ToString());
                        for (int variable2 = 0; variable2 <= bound2; variable2++)
                        {
                            double? value = starteEndnodes[variable0, variable1, variable2];
                            buildAstring.Append(",[" + variable2.ToString() + "," + value + "]");
                        }
                        buildAstring.Append("]");
                    }
                    buildAstring.Append("]" + Environment.NewLine);
                } else
                {
                    break;
                }
            }
            return buildAstring.ToString();
        }

        public void drawPixel(int x, int y, int size, int redValue, int greenValue, int blueValue)
        {
            Color myColor = Color.FromArgb(redValue, greenValue, blueValue);
            SolidBrush brushColor = new SolidBrush(myColor);
            Graphics g = mainForm.CreateGraphics();
            if (x != 0 && y != 0)
            {
                g.FillRectangle(brushColor, x, y, size, size);
            }
        }

        public void removePixel(int x, int y, int size)
        {

            Brush aBrush = (Brush)Brushes.White;
            Graphics g = mainForm.CreateGraphics();
            if (x != 0 && y != 0)
            {
                try
                {
                    g.FillRectangle(aBrush, x, y, size, size);
                } catch (Exception e)
                {
                    DebugCallBack("derp");
                }
            }

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
                        for (count = (int)y2; count <= y1; count++)
                        {

                            if (!remove)
                            {
                                drawPixel((int)x, count, size, redValue, greenValue, blueValue);
                            }
                            else
                            {
                                removePixel((int)x, count, size);
                            }
                        }

                    }
                    else
                    {
                        y = Math.Round(((y2 - y1) / (x2 - x1)) * (x - x1) + y1);
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
                }
                else if (prevY - y > 0 && x != 0)
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
                }
                else
                {
                    removePixel((int)x, (int)y, size);
                }

                prevY = y;
                prevX = x;
            }
        }


        public void drawAxMatrix(int screen, int r, int g, int b, int size)
        {
            drawLine(globalScreenInformation[screen][0], 0, -globalScreenInformation[screen][0], 0, globalScreenInformation[screen][0], globalScreenInformation[screen][1], r, g, b, size, false);
            drawLine(0, globalScreenInformation[screen][1], 0, -globalScreenInformation[screen][1], globalScreenInformation[screen][0], globalScreenInformation[screen][1], r, g, b, size, false);
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

        public double prevousZoomScreenTL = 0;
        public double prevousZoomScreenTR = 0;
        public double prevousZoomScreenBR = 0;
        public double prevousZoomScreenBL = 0;
        public void drawNodes(int conv3dto2d, int screen, double zoomscreen, int rotation)
        {
            isStillDrawing = true;
            int linesDrawn;
            double startx = 0;
            double starty = 0;
            double startz = 0;
            double endx = 0;
            double endy = 0;
            double endz = 0;

            

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
                            case 4:
                                if (zoomscreen != prevousZoomScreenBR)
                                {
                                    removePixel((int)(((startx * prevousZoomScreenBR) + globalScreenInformation[screen][0])), (int)((globalScreenInformation[screen][1] - (starty * prevousZoomScreenBR))), 4);
                                }
                                break;
                        }

                        if (selectedNodes[selectedcounter] != null)
                        {
                            if (selectedNodes[selectedcounter].Value == i)
                            {
                                drawPixel((int)(((startx * zoomscreen) + globalScreenInformation[screen][0])), (int)((globalScreenInformation[screen][1] - (starty * zoomscreen))), 4, 0, 255, 0);
                                break;
                            }
                            else
                            {
                                drawPixel((int)(((startx * zoomscreen) + globalScreenInformation[screen][0])), (int)((globalScreenInformation[screen][1] - (starty * zoomscreen))), 4, 255, 0, 0);
                            }
                        }
                        else
                        {
                            drawPixel((int)(((startx * zoomscreen) + globalScreenInformation[screen][0])), (int)((globalScreenInformation[screen][1] - (starty * zoomscreen))), 4, 255, 0, 0);
                        }
                    }
                }
            }


            for (linesDrawn = 0; linesDrawn < maxLines; linesDrawn++)
            {
                if (starteEndnodes[linesDrawn, 0, 0] != null && starteEndnodes[linesDrawn, 0, 1] != null && starteEndnodes[linesDrawn, 1, 0] != null && starteEndnodes[linesDrawn, 1, 1] != null)
                {
                    startx = starteEndnodes[linesDrawn, 0, 0].Value;
                    starty = starteEndnodes[linesDrawn, 0, 1].Value;
                    startz = starteEndnodes[linesDrawn, 0, 2].Value;
                    endx = starteEndnodes[linesDrawn, 1, 0].Value;
                    endy = starteEndnodes[linesDrawn, 1, 1].Value;
                    endz = starteEndnodes[linesDrawn, 1, 2].Value;

                    double rstartx = starteEndnodes[linesDrawn, 0, 0].Value;
                    double rendx = starteEndnodes[linesDrawn, 1, 0].Value;
                    double rstarty = starteEndnodes[linesDrawn, 0, 1].Value;
                    double rstartz = starteEndnodes[linesDrawn, 0, 2].Value;
                    double rendy = starteEndnodes[linesDrawn, 1, 1].Value;
                    double rendz = starteEndnodes[linesDrawn, 1, 2].Value;


                    if (conv3dto2d == 0)
                    {

                        double rotationdifferencestart = (((startx * -1) - (starty * -1)) / 90);
                        double rotationdifferenceend = (((endx * -1) - (endy * -1)) / 90);
                        startx = rstartx + (rotationdifferencestart * rotation);
                        endx = rendx + (rotationdifferenceend * rotation);
                        starty = startz;
                        endy = endz;
                    }
                    else if (conv3dto2d == 1)
                    {

                        double rotationdifferencestart = (((startx * -1) - (startz * -1)) / 90);
                        double rotationdifferenceend = (((endx * -1) - (endz * -1)) / 90);
                        startx = - (rstartx + (rotationdifferencestart * rotation));
                        starty = startz;
                        endx = -(rendx + (rotationdifferenceend * rotation));
                        endy = endz;
                    } else if(conv3dto2d == 2)
                    {
                        double rotationdifferencestart = (((startx * -1) - (startz * -1)) / 90);
                        double rotationdifferenceend = (((endx * -1) - (endz * -1)) / 90);
                        startx = rstartx + (rotationdifferencestart * rotation);
                        endx = rendx + (rotationdifferenceend * rotation);

                    }
                    else if (conv3dto2d == 3)
                    {

                        double rotationdifferencestart = (((startx * -1) - (startz * -1)) / 90);
                        double rotationdifferenceend = (((endx * -1) - (endz * -1)) / 90);
                        startx = -startx;
                        endx = -endx;
                    }
                    else if (conv3dto2d == 4)
                    {

                        double rotationdifferencestart = (((startz * -1) - (startx * -1)) / 90);
                        double rotationdifferenceend = (((endz * -1) - (endx * -1)) / 90);
                        startx = (rstartz + (rotationdifferencestart * rotation));
                        endx = (rendz + (rotationdifferenceend * rotation));
                    }
                    else if (conv3dto2d == 5)
                    {

                        double rotationdifferencestart = (((startz * -1) - (startx * -1)) / 90);
                        double rotationdifferenceend = (((endz * -1) - (endx * -1)) / 90);
                        startx = -(rstartz + (rotationdifferenceend * rotation));
                        endx = -(rendz + (rotationdifferenceend * rotation));
                    }

                    switch (screen)
                    {
                        case 1:
                            if (zoomscreen != prevousZoomScreenTL)
                            {
                                drawLine((startx * prevousZoomScreenTL), (starty * prevousZoomScreenTL), (endx * prevousZoomScreenTL), (endy * prevousZoomScreenTL), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 255, 255, 255, 1, true);
                                drawLine((startx * zoomscreen), (starty * zoomscreen), (endx * zoomscreen), (endy * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);

                            } else
                            {
                                drawLine((startx * zoomscreen), (starty * zoomscreen), (endx * zoomscreen), (endy * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);

                            }


                            break;
                        case 2:
                            if (zoomscreen != prevousZoomScreenTR)
                            {
                                drawLine((startx * prevousZoomScreenTR), (starty * prevousZoomScreenTR), (endx * prevousZoomScreenTR), (endy * prevousZoomScreenTR), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 255, 255, 255, 1, true);
                                
                                drawLine((startx * zoomscreen), (starty * zoomscreen), (endx * zoomscreen), (endy * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);

                            }
                            else
                            {
                                drawLine((startx * zoomscreen), (starty * zoomscreen), (endx * zoomscreen), (endy * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);

                            }
                            break;
                        case 3:
                            if (zoomscreen != prevousZoomScreenBL)
                            {
                                drawLine((startx * prevousZoomScreenBL), (starty * prevousZoomScreenBL), (endx * prevousZoomScreenBL), (endy * prevousZoomScreenBL), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 255, 255, 255, 1, true);
                             
                                drawLine((startx * zoomscreen), (starty * zoomscreen), (endx * zoomscreen), (endy * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);
                               
                            }
                            else
                            {
                                drawLine((startx * zoomscreen), (starty * zoomscreen), (endx * zoomscreen), (endy * zoomscreen), globalScreenInformation[screen][0], globalScreenInformation[screen][1], 0, 0, 0, 1, false);

                            }

                            break;
                        case 4:
                           
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
                    break;
            }
            isStillDrawing = false;
        }
    }
}