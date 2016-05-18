#include <stdio.h>
#include "includes.h"
#include <os/alt_sem.h>
/* Definition of Task Stacks */
#define   TASK_STACKSIZE       4096             //we need such a big stacksize because we save sin(0-360) and cos(0-360) in there to get a better performance.
OS_STK    TaskSerialInput_stk[TASK_STACKSIZE];
OS_STK    TaskDraw3D_stk[TASK_STACKSIZE];
OS_STK    TaskStartUp_stk[TASK_STACKSIZE];
OS_STK    TaskFPSCounter_stk[TASK_STACKSIZE];

/* Definition of Task Priorities */

#define TaskSerialInput_PRIORITY    1
#define TaskFPSCounter_PRIORITY     2
#define TaskStartUp_PRIORITY        3
#define TaskDraw3D_PRIORITY         4
#define VALUES 240              //the maximum value the array can contain. (240/6 == 40 Lines in total)

ALT_SEM(sem_objectDrawn)        //semaphore to prevent that multiple tasks use the same global variables at the same time.
ALT_SEM(sem_updateFps)          //sempahore to prevent that the global variable FPScounter is used at the same time by multiple tasks.

//globale variable of the object array, zoom, rotation, amount of lines, fpscounter.

float *zoomscreenpointer;
int *startEndnodes;
int *rotation;
int amountOfLines;
int stopDrawing = 1;        //we made this variable because you can't change the rotation and zoom if the object isn't fully drawn yet.
int fpsCounter  = 0;
int offscreen   = 0;        //we made this variable so we could stop the function drawLine to draw lines. We did this to check the performance.


//Write text on the screen.
void VGA_text(int x, int y, char * text_ptr)
{
	int offset;
  	volatile char * character_buffer = (char *) 0x09000000;	// VGA character buffer

	/* assume that the text string fits on one line */
	offset = (y << 7) + x;
	while ( *(text_ptr) )
	{
		*(character_buffer + offset) = *(text_ptr);	// write to the character buffer
		++text_ptr;
		++offset;
	}
}


//draw a box on screen.
void VGA_box(int x1, int y1, int x2, int y2, short pixel_color)
{
	int offset, row, col;
  	volatile short * pixel_buffer = (short *) 0x08000000;	// VGA pixel buffer

	/* assume that the box coordinates are valid */
	for (row = y1; row <= y2; row++)
	{
		col = x1;
		while (col <= x2)
		{
			offset = (row << 9) + col;
			*(pixel_buffer + offset) = pixel_color;	// compute halfword address, set pixel
			++col;
		}
	}
}

//reset screen to a black screen. Removes all boxes and text.
void resetScreen(){

	VGA_box (0, 0, 319, 239, 0);
	int i;
	for(i = 0; i < 70; i++){
		int j;
		for(j = 0; j < 60; j++){
			VGA_text (i, j, " ");
		}
	}
}

//draws 1 pixel on the screen.
void drawPixel(int x, int y, int size, short pixel_color )
{
	if(x != 0){
		VGA_box (x, y, x + size - 1, y + size - 1, pixel_color);
	}
}

//removes 1 pixel off the screen
void removePixel(int x, int y, int size)
{
	VGA_box (x, y, x +(size - 1), y + (size - 1), 0x0000);
}

//fnction to draw a line from x1, y1 to x2,y2.
void drawLine(float x1, float y1, float x2, float y2, float midX, float midY, int size, int remove, short pixel_color)
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
    float x = 0;
    float y = 0;
    float prevY = 0;
    float prevX = 0;
    //for loop for drawing beam
    for (x = x1; x <= x2; x++)
    {
    	if(y1 != y2)
    	{
    		y = 0;
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
                        drawPixel((int)x, (int)count, size, pixel_color);
                    }
                    else
                    {
                        removePixel((int)x, (int)count, size);
                    }
                }
            }
            else
            {
            	float a = y2 - y1;
            	float b = x2 - x1;
            	float c = a / b;
            	float d = x - x1;
            	float e = c * d;
                y = e + y1;
            }
		 //drawing all pixels of beam between nodes
			if (y - prevY > 0 && x != 0)
			{

				float counter;
				for (counter = prevY; counter <= y; counter++)
				{
					if (!remove)
					{
						drawPixel((int)prevX, (int)counter, size, pixel_color);
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
						drawPixel((int)prevX, (int)counter, size, pixel_color);
					}
					else
					{
						removePixel((int)prevX, (int)counter, size);
					}
				}
			}
			prevY = y;
			prevX = x;
        } else if(x1 == x2){
        	if(y1 < y2){
        		int y = y1;
				for(y = y1; y <= y2; y++){
					if (!remove)
					{
						drawPixel((int)x1, (int)y, size, pixel_color);
					}
					else
					{
						removePixel((int)x1, (int)y, size);
					}
				}
        	} else {
        		int y = y2;
				for(y = y2; y <= y1; y++){
					if (!remove)
					{
						drawPixel((int)x1, (int)y, size, pixel_color);
					}
					else
					{
						removePixel((int)x1, (int)y, size);
					}
				}
        	}
        } else {

			if (!remove)
			{
				drawPixel((int)x, (int)y1, size, pixel_color);
			}
			else
			{
				removePixel((int)x, (int)y1, size);
			}
        }
    }
}

//Task for sending information from the simulation/editor to the FPGA. Also the task to change rotation and zoom by using the switches.
void TaskSerialInput(void* pdata)
{
    //value of current switches enabled
    int SWITCH_value;
    
    //pointers for accessing data from other hardware
	volatile int* UART_DATA_ptr = (int*) 0x10001010; 
	volatile int* UART_CONTROL_ptr = (int *) 0x10001014;
	volatile int * SWITCH_ptr = (int *) 0x10000040;
    
    //values for rotation and zoom
	int xRotation = rotation[0];
	int yRotation = rotation[1];
	int zRotation = rotation[2];
	float zoom = *zoomscreenpointer;

	//values necessary to calculate receive progress and drawing progress bar
	int amountOfIntegersReceived = 0;
	float totalAmountOfCharactersToBeSend = 0;
	float totalAmountOfCharactersReceived = 0;
	float oneProcentOfCharactersToBeSend = 0;
	float loadingBarOneProcent = 2.4;
	
	//contains received integer (to be combined)
	char integer[4];
	
	//tells the program if value in integer array is a 1, 10, 100 or 1000 number
	int integerPos = 0;
    
    //resets the array
	int test[VALUES];
	startEndnodes = malloc(sizeof(int) * VALUES);
	startEndnodes = test;
	
	//contains data received from serial
	int dataSerial;
    while (1)
    {
	    printf("Start task Serial Input \n");

    
	  	while(1){

			SWITCH_value = *(SWITCH_ptr);
	  		ALT_SEM_PEND(sem_objectDrawn, 0);
	  		dataSerial = *(UART_DATA_ptr);
	  		if(dataSerial & 0x8000){

	  			if(stopDrawing){        //only run this if the Taskdraw3D is done with drawing.
	  				int pixelPos = (int)((totalAmountOfCharactersReceived * oneProcentOfCharactersToBeSend) * loadingBarOneProcent);
	  				int x = 0;
	  				for(x = 0; x < (int)pixelPos; x++){
	  					drawPixel(20 + x,162, 7, 0x0F00 );
	  					char text_bottom_row1[40];
						sprintf(text_bottom_row1,  "%d %c",(int)(totalAmountOfCharactersReceived * oneProcentOfCharactersToBeSend), '%');
						VGA_text (35, 41, text_bottom_row1);
	  				}

	  			}



	  			char customString[2] = {(char)dataSerial, '\0'};
	  			//if the FPGA receives a 't' he knows how many characters will come.
	  			if(customString[0] == 't'){
	  				if(integerPos > 0){

						totalAmountOfCharactersToBeSend = 0;
						totalAmountOfCharactersReceived = 0;
						oneProcentOfCharactersToBeSend = 0;
						int total;
						if(integerPos == 1){
							total = integer[0];
						} else if(integerPos == 2){
							total = (integer[0] * 10 + integer[1]);
						} else if(integerPos == 3){
							total = (integer[0] * 100 + integer[1] * 10 + integer[2]);
						} else if(integerPos == 4){
							total = (integer[0] * 1000 + integer[1] * 100 + integer[2] * 10 + integer[3]);
						}else {
							total = 1;
						}
						totalAmountOfCharactersToBeSend = total;
						oneProcentOfCharactersToBeSend = 100 / totalAmountOfCharactersToBeSend;
						
						integerPos = 0;
					}
				//if the FPGA receives a 'n' he knows that there will come a new object
	  			}else if(customString[0] == 'n'){

	  				resetScreen();
	  				int i;

	  				char text_top_row[40] = "NEON 3D Inc.";
					VGA_text(1,0,text_top_row);
					char text_bottom_row3[40] = "SOFTWARE MODE \0";
					VGA_text (56, 0, text_bottom_row3);
	  				test[VALUES];
	  				for(i = 0; i < VALUES; i++){
	  					test[i] = 0;
	  					//printf("reset index: %d \n", i);
	  					(startEndnodes)[i] = 0;
	  				}

					startEndnodes = test;           
					amountOfLines = 0;
					integerPos = 0;                 
					amountOfIntegersReceived = 0;   

					xRotation = 0;
					rotation[0] = xRotation;
					yRotation = 0;
					rotation[1] = yRotation;
					zRotation = 0;
					rotation[2] = zRotation;
					
					*zoomscreenpointer = zoom;
					
					stopDrawing = 1;
					
					VGA_text(31, 30, "LOADING OBJECT");
				//if the FPGA receives a 'd' or a ' ' he knows that he received all characters and the taskDraw3D can start drawing the object.
	  			} else if(customString[0] == 'd' || customString[0] == ' '){
	  				resetScreen();

	  				char text_top_row[40] = "NEON 3D Inc.";
	  				VGA_text(1,0,text_top_row);
	  				char text_bottom_row3[40] = "SOFTWARE MODE \0";
					VGA_text (56, 0, text_bottom_row3);
	  				int amountOfLines = amountOfIntegersReceived / 6;
					char text_bottom_row1[40];
					sprintf(text_bottom_row1, "3D amount of lines: %d", amountOfLines );
					VGA_text (1, 1, text_bottom_row1);

					totalAmountOfCharactersToBeSend = 0;
					totalAmountOfCharactersReceived = 0;
					oneProcentOfCharactersToBeSend = 0;
					loadingBarOneProcent = 2.4;
					startEndnodes = test;
	  				stopDrawing = 0;
	  			//if the FPGA receives a 'm' he knows that he received a coordination which is lower than 0.
	  			} else if(customString[0] == 'm'){

					if(integerPos > 0){

						int total;
						if(integerPos == 1){
							total = integer[0] * -1;
						} else if(integerPos == 2){
							total = (integer[0] * 10 + integer[1]) * -1;
						} else if(integerPos == 3){
							total = (integer[0] * 100 + integer[1] * 10 + integer[2]) * -1;
						} else if(integerPos == 4){
							total = (integer[0] * 1000 + integer[1] * 100 + integer[2] * 10 + integer[3]) * -1;
						}
						test[amountOfIntegersReceived] = total;
						amountOfIntegersReceived++;


						int amountOfLines = amountOfIntegersReceived / 6;
						char text_bottom_row1[40];
						sprintf(text_bottom_row1, "3D amount of lines: %d", amountOfLines );
						VGA_text (1, 1, text_bottom_row1);
						
						integerPos = 0;
					}

	  			}
	  			//if the FPGA receives a 'p' he knows that he received a coordination which is bigger than 0.
	  			else if(customString[0] == 'p'){
	  				if(integerPos > 0){

						int total;
						if(integerPos == 1){
							total = integer[0];
						} else if(integerPos == 2){
							total = (integer[0] * 10 + integer[1]);
						} else if(integerPos == 3){
							total = (integer[0] * 100 + integer[1] * 10 + integer[2]);
						} else if(integerPos == 4){
							total = (integer[0] * 1000 + integer[1] * 100 + integer[2] * 10 + integer[3]);
						}
						test[amountOfIntegersReceived] = total;

						amountOfIntegersReceived++;

						int amountOfLines = amountOfIntegersReceived / 6;
						char text_bottom_row1[40];
						sprintf(text_bottom_row1, "3D amount of lines: %d", amountOfLines );
						VGA_text (1, 1, text_bottom_row1);
						integerPos = 0;
					}
                //if the PGA recives a 'u' y rotation will increase with 1.
	  			} else if(customString[0] == 'u'){
	  				yRotation = yRotation + 1;
					if(yRotation > 359){
						yRotation = 0;
					}
					rotation[1] = yRotation;
				//if the PGA recives a 'b' y rotation will decrease with 1.
	  			} else if(customString[0] == 'b'){
	  				yRotation = yRotation - 1;
					if(yRotation < 1){
						yRotation = 360;
					}
					rotation[1] = yRotation;
                //if the PGA recives a 'l' x rotation will decrease with 1.
	  			} else if(customString[0] == 'l'){
	  				xRotation = xRotation - 1;
					if(xRotation < 1){
						xRotation = 360;
					}
					rotation[0] = xRotation;
				//if the PGA recives a 'r' x rotation will increase with 1.
	  			} else if(customString[0] == 'r'){
	  				xRotation = xRotation + 1;
					if(xRotation > 359){
						xRotation = 0;
					}
					rotation[0] = xRotation;
				//if the PGA recives a 'x' z rotation will decrease with 1.
	  			} else if(customString[0] == 'x'){
					zRotation = zRotation - 1;
					if(zRotation < 1){
						zRotation = 360;
					}
					rotation[2] = zRotation;
				//if the PGA recives a 'z' z rotation will increase with 1.
				} else if(customString[0] == 'z'){
					zRotation = zRotation + 1;
					if(zRotation > 359){
						zRotation = 0;
					}
					rotation[2] = zRotation;
			    //if the FPGA receives 's' rotation and zoom will be set to default values.
				} else if(customString[0] == 's'){

					xRotation = 0;
					rotation[0] = xRotation;
	  				yRotation = 0;
					rotation[1] = yRotation;
					zRotation = 0;
					rotation[2] = zRotation;
					zoom = 0.8;
					*zoomscreenpointer = zoom;
	  			}
	  			//if the FPGA receives something else he knows it is a character between 0 and 9.
	  			else {
	  				integer[integerPos] = customString[0] - '0';
					integerPos++;
	  			}

	  			if(stopDrawing){
		  			totalAmountOfCharactersReceived++;
	  			}

				*UART_DATA_ptr = *(UART_DATA_ptr) ^ (0x0000 ^ 'Y');
	  		}

	  		if(!stopDrawing){
	  		    //switch 0 sets rotation and zoom to default values.
	  			if (SWITCH_value & 0x1)					// check SWITCH0
				{
					yRotation = 0;
					rotation[1] = yRotation;
					xRotation = 0;
					rotation[0] = xRotation;
					zRotation = 0;
					rotation[2] = zRotation;
					zoom = 0.8;
					*zoomscreenpointer = zoom;
				}
				//switch 1 increases y rotation with 1.
				if (SWITCH_value & 0x2)					
				{
					yRotation = yRotation + 1;
					if(yRotation > 359){
						yRotation = 0;
					}
					rotation[1] = yRotation;

				}
				//switch 2 decreases y rotation with 1
				if (SWITCH_value & 0x4)					
				{
					yRotation = yRotation - 1;
					if(yRotation < 1){
						yRotation = 360;
					}
					rotation[1] = yRotation;
				}
				//switch 3 increases x rotation with 1.
				if (SWITCH_value & 0x8)					
				{
					xRotation = xRotation + 1;
					if(xRotation > 359){
						xRotation = 0;
					}
					rotation[0] = xRotation;
				}
				//switch 4 decreases x rotation with 1.
				if (SWITCH_value & 0x10)					
				{
					xRotation = xRotation - 1;
					if(xRotation < 1){
						xRotation = 360;
					}
					rotation[0] = xRotation;
				}
                //switch 5 decreases z rotation with 1.
				if (SWITCH_value & 0x20)					
				{
					zRotation = zRotation - 1;
					if(zRotation < 1){
						zRotation = 360;
					}
					rotation[2] = zRotation;
				}
                //switch 6 increases z rotation with 1.
				if (SWITCH_value & 0x40)					
				{
					zRotation = zRotation + 1;
					if(zRotation > 359){
						zRotation = 0;
					}
					rotation[2] = zRotation;
				}
                //switch 7 will pause the drawLines function to test the performance.
				if (SWITCH_value & 0x80)					
				{

					VGA_text(31, 30, "OFFSCREEN BENCHMARK");
					offscreen = 1;
				} else {
					VGA_text(31, 29, "                     ");
					offscreen = 0;
				}
				//switch 16 decreases zoom with 0.01
				if(SWITCH_value & 0x10000){
					if(zoom >= 0.11){
						zoom = zoom - 0.01;
						*zoomscreenpointer = zoom;
					}
				}
                //switch 17 increases zoom with 0.01
				if(SWITCH_value & 0x20000){
					if(zoom <= 1.49){
						zoom = zoom + 0.01;
						*zoomscreenpointer = zoom;
					}
				}
	  		}
            //set text on screen.
	  		char onScreenZoom[40];
	  		char onScreenRotation[40];
			VGA_text (6, 2, "                                          \0");
			VGA_text (15, 3, "                                          \0");
			sprintf(onScreenZoom, "Zoom: %f", *zoomscreenpointer);
			VGA_text (1, 2, onScreenZoom);
			sprintf(onScreenRotation, "Rotation X,Y,Z: %d,%d,%d", rotation[0], rotation[1], rotation[2]);
			VGA_text (1, 3, onScreenRotation);

	  		ALT_SEM_POST(sem_objectDrawn);
	  		OSTimeDly(1);
	  	}
  }
}


//Reads the startEndNodes array and performs the rotation calculations on it while pushing the caclulated values to the drawLine function.
void TaskDraw3D(void* pdata)
{
	printf("Start task draw 3D");

    //to make sure nothing is on the screen before drawing starts.
    resetScreen();
	
	//default line color
	short color = 0x0F00;
	
	//midpoint of screen
	int midX = (320/2);
	int midY = (240/2);
	
	//for erasing previous rotated/zoomed object
	float prevousZoomScreenBR = 0;
	int previousXRotation = 0;
	int previousYRotation = 0;
	int previousZRotation = 0;

	char onScreentitle[40] = "NEON 3D Inc.";
	char onScreenMode[40] = "SOFTWARE MODE \0";
	char onScreenAmountOfLines[40];
	sprintf(onScreenAmountOfLines, "3D amount of lines: %d", 0);

	VGA_text(1,0, onScreenTitle);
	VGA_text (1, 1, onScreenAmountOfLines);
	VGA_text (56, 0, onScreenMode);

    //calculate all sin/cos values for given rotation
	float sinArray[360];
	float cosArray[360];
	
	int i;
	for(i = 0; i <= 360; i++){
		sinArray[i] = sin(i / 57.4);
		cosArray[i] = cos(i / 57.4);
	}
    
    
	//local array to perform rotation and zoom calculations on
	int temporaryStartEndNodes[VALUES];

	while(1){
	    //wait for other task to stop accessing global values such as rotation, zoom and startendnodes array
		ALT_SEM_PEND(sem_objectDrawn, 0);
        
        //local start and end node values
		float startx = 0;
		float starty = 0;
		float startz = 0;
		float endx = 0;
		float endy = 0;
		float endz = 0;

        //local rotation values
		int localRotationX = rotation[0];
		int localRotationY = rotation[1];
		int localRotationZ = rotation[2];
		float zoomscreen = *zoomscreenpointer;
        
        //local array to perform calculations on (to be removed?)
		for(i = 0;  i < VALUES; i++){
			temporaryStartEndNodes[i] = (startEndnodes)[i];
		}
		startEndnodes = temporaryStartEndNodes;
        
        //check if it should stop drawing/stop performing calculations on array (set in TaskSerialInput)
		if(!stopDrawing){
		    
		    int linesDrawn;
			for (linesDrawn = 0; linesDrawn < VALUES/6; linesDrawn++)
			{

                //skips index every interation by 6
				int indexOfArray = linesDrawn * 6;
				
				//read start node from global array
				float x1 = startx = (startEndnodes)[indexOfArray];
				float y1 = starty = (startEndnodes)[indexOfArray + 1];
				float z1 = startz = (startEndnodes)[indexOfArray + 2];

                //read end node from global array
				float x2 = endx = (startEndnodes)[indexOfArray + 3];
				float y2 = endy = (startEndnodes)[indexOfArray + 4];
				float z2 = endz = (startEndnodes)[indexOfArray + 5];

                //if start and end node are both zero (if array has less then 240 elements) stop the draw loop and do not perform calculations
				if(x1 == 0 && x2 == 0 && y1 == 0 && y2 == 0 && z1 == 0 && z2 == 0){
					break;
				}
                
                //if a change happend in either the rotation or zoom, remove the previous known object calculated with the previous known zoom and rotation from screen
				if(previousXRotation != localRotationX || previousYRotation != localRotationY || previousZRotation != localRotationZ || zoomscreen != prevousZoomScreenBR)
				{
				     //perform rotation calculations on the 3 dimensonal nodes and convert them to 2D nodes and then perform zoom calculation.
					 x1 = (((startx * cosArray[previousXRotation] - (starty * cosArray[previousZRotation] - startz * sinArray[previousZRotation]) * sinArray[previousXRotation]) * (cosArray[previousYRotation])) + (starty * sinArray[previousZRotation] + startz * cosArray[previousZRotation]) * sinArray[previousYRotation]) * prevousZoomScreenBR;
					 x2 = (((endx * cosArray[previousXRotation] - (endy * cosArray[previousZRotation] - endz * sinArray[previousZRotation]) * sinArray[previousXRotation]) * (cosArray[previousYRotation])) + (endy * sinArray[previousZRotation] + endz * cosArray[previousZRotation]) * sinArray[previousYRotation]) * prevousZoomScreenBR;

					 y1 = (((-(startx * cosArray[previousXRotation] - (starty * cosArray[previousZRotation] - startz * sinArray[previousZRotation]) * sinArray[previousXRotation])) * (sinArray[previousYRotation]) + (starty * sinArray[previousZRotation] + startz * cosArray[previousZRotation]) * cosArray[previousYRotation])) * prevousZoomScreenBR;
					 y2 = (((-(endx * cosArray[previousXRotation] - (endy * cosArray[previousZRotation] - endz * sinArray[previousZRotation]) * sinArray[previousXRotation])) * (sinArray[previousYRotation]) + (endy * sinArray[previousZRotation] + endz * cosArray[previousZRotation]) * cosArray[previousYRotation])) * prevousZoomScreenBR;
                     
                     //if offscreen benchmark mode is enabled, do not execute draw functions (which is the current bottleneck)
					 if(!offscreen){
					     
						 drawLine(x1,y1,x2,y2, midX, midY,1 , 1, color);
					 }

				}
				
               //perform rotation calculations on the 3 dimensonal nodes and convert them to 2D nodes and then perform zoom calculation.
				 x1 = (((startx * cosArray[localRotationX] - (starty * cosArray[localRotationZ] - startz * sinArray[localRotationZ]) * sinArray[localRotationX]) * (cosArray[localRotationY])) + (starty * sinArray[localRotationZ] + startz * cosArray[localRotationZ]) * sinArray[localRotationY]) * zoomscreen;
				 x2 = (((endx * cosArray[localRotationX] - (endy * cosArray[localRotationZ] - endz * sinArray[localRotationZ]) * sinArray[localRotationX]) * (cosArray[localRotationY])) + (endy * sinArray[localRotationZ] + endz * cosArray[localRotationZ]) * sinArray[localRotationY]) * zoomscreen;

				 y1 = (((-(startx * cosArray[localRotationX] - (starty * cosArray[localRotationZ] - startz * sinArray[localRotationZ]) * sinArray[localRotationX])) * (sinArray[localRotationY]) + (starty * sinArray[localRotationZ] + startz * cosArray[localRotationZ]) * cosArray[localRotationY]))* zoomscreen;
				 y2 = (((-(endx * cosArray[localRotationX] - (endy * cosArray[localRotationZ] - endz * sinArray[localRotationZ]) * sinArray[localRotationX])) * (sinArray[localRotationY]) + (endy * sinArray[localRotationZ] + endz * cosArray[localRotationZ]) * cosArray[localRotationY]))* zoomscreen;

                //if offscreen benchmark mode is enabled, do not execute draw functions (which is the current bottleneck)
				if(!offscreen){
					drawLine(x1,y1,x2,y2, midX, midY, 1 , 0, color);
				}
			}
            
            //save previous zoomscreen if its different then before
			if (zoomscreen != prevousZoomScreenBR)
			{
				prevousZoomScreenBR = zoomscreen;
			}
            
            //save previous rotation
			previousXRotation = localRotationX;
			previousYRotation = localRotationY;
			previousZRotation = localRotationZ;
			
			//update fps counter (after for loop is finished peforming calculation, a complete frame has been drawn)
			ALT_SEM_PEND(sem_updateFps, 0);
			fpsCounter++;
			ALT_SEM_POST(sem_updateFps);

		}
        //other tasks may perform actions on global values used in this task
		ALT_SEM_POST(sem_objectDrawn);
	}
}

//On startup, this task shows the Text Neon3D as a 3D object, and rotates it over the Y axle. It deletes itself after its finsished
void TaskStartUp(void* pdata){
    
	printf("Start StartUp task");
    
    //set startup values
	amountOfLines = 0;
	int rotatevalue[3] = {0,0,0};
	rotation = malloc(sizeof(int)*3);
	rotation = rotatevalue;

	float zoom = 0.5;
	zoomscreenpointer = malloc(sizeof(float));
	zoomscreenpointer = &zoom;
    
    //reset screen to remove any pixels or text thats not meant to be there
	resetScreen();
    
    //hardcoded node3d text as 3D object
	int test[VALUES] = {-48,0,-2,-48,39,0,-30,0,-2,-30,39,0,-19,0,0,-8,38,0,-20,-39,0,-8,18,0-21,18,0,-8,0,0,-19,0,0,6,37,0,6,0,0,25,0,0,37,0,6,37,0,25,37,0,39,0,0,38,37,0,38,37,0,56,0,0,56,0,0,56,37,0,0,-53,0,-24,-53,0,0,-53,0,0,-37,0,-23,-33,0,0,-13,0,-23,-12,0,0,-37,0,0,-13,0,0,-55,0,20,-56,0,0,-36,0,22,-35,0,0,-55,0,0,-36,0,22,-35,0,20,-56,0,22,-35,0,20,-56,0,22,-13,0,22,-35,0};
	startEndnodes = malloc(sizeof(int) * VALUES);
	startEndnodes = test;

	//local rotation counter
	int zRotation;
	for(zRotation = 0; zRotation <= 90; zRotation++){
		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[2] = zRotation;
		char onScreenZoom[40];
		char onScreenRotation[40];
		VGA_text (6, 2, "                                          \0");
		VGA_text (15, 3, "                                          \0");
		sprintf(onScreenZoom, "Zoom: %f", *zoomscreenpointer);
		VGA_text (1, 2, onSreenZoom);
		sprintf(onScreenRotation, "Rotation X,Y,Z: %d,%d,%d", rotation[0], rotation[1], rotation[2]);
		VGA_text (1, 3, onScreenRotation);

		OSTimeDlyHMSM(0,0,0,500);

		ALT_SEM_POST(sem_objectDrawn);
	}
	
	OSTaskDel(OS_PRIO_SELF);
}


//Prints every second the value of fpsCounter (which is incremented in TaskDraw3D) and resets it.
void TaskFPSCounter(void* pdata){

	printf("FPS COUNTER RUNS");
	while(1){
		ALT_SEM_PEND(sem_updateFps, 0);
		char onScreenFps[40];
		sprintf(onScreenFps, "FPS: %d", fpsCounter);
		VGA_text (1, 58, "                                     ");
		VGA_text (1, 58, onScreenFps);
		fpsCounter = 0;
		ALT_SEM_POST(sem_updateFps);
		OSTimeDlyHMSM(0,0,1,0);
	}
}


/* The main function creates two task and starts multi-tasking */
int main(void)
{

	int err = ALT_SEM_CREATE(&sem_objectDrawn, 1);
	err = ALT_SEM_CREATE(&sem_updateFps, 1);
	//gets serial input and switch input
	OSTaskCreateExt(TaskSerialInput,
                  NULL,
                  (void *)&TaskSerialInput_stk[TASK_STACKSIZE-1],
                  TaskSerialInput_PRIORITY,
                  TaskSerialInput_PRIORITY,
                  TaskSerialInput_stk,
                  TASK_STACKSIZE,
                  NULL,
                  0);

	//draws 3D object on screen
	OSTaskCreateExt(TaskDraw3D,
                  NULL,
                  (void *)&TaskDraw3D_stk[TASK_STACKSIZE-1],
                  TaskDraw3D_PRIORITY,
                  TaskDraw3D_PRIORITY,
                  TaskDraw3D_stk,
                  TASK_STACKSIZE,
                  NULL,
                  0);

	//default start screen

	/*
	OSTaskCreateExt(TaskStartUp,
	                  NULL,
	                  (void *)&TaskStartUp_stk[TASK_STACKSIZE-1],
	                  TaskStartUp_PRIORITY,
	                  TaskStartUp_PRIORITY,
	                  TaskStartUp_stk,
	                  TASK_STACKSIZE,
	                  NULL,
	                  0);*/
	                  
    //counts frames per second.
	OSTaskCreateExt(TaskFPSCounter,
	                  NULL,
	                  (void *)&TaskFPSCounter_stk[TASK_STACKSIZE-1],
	                  TaskFPSCounter_PRIORITY,
	                  TaskFPSCounter_PRIORITY,
	                  TaskFPSCounter_stk,
	                  TASK_STACKSIZE,
	                  NULL,
	                  0);
    OSStart();
  return 0;
}
