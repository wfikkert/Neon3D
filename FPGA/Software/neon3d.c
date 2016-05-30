#include <stdio.h>
#include "includes.h"
#include <os/alt_sem.h>

#define M_PI 3.14159265358979323846
/* Definition of Task Stacks */
#define   TASK_STACKSIZE       4096             //we need such a big stacksize because we save sin(0-360) and cos(0-360) in there to get a better performance.
OS_STK    TaskSerialInput_stk[TASK_STACKSIZE];
OS_STK    TaskDraw3D_stk[TASK_STACKSIZE];
OS_STK    TaskStartUp_stk[TASK_STACKSIZE];
OS_STK    TaskBenchmark_stk[TASK_STACKSIZE];
OS_STK    TaskFPSCounter_stk[TASK_STACKSIZE];
OS_STK    TaskDraw3D2_stk[TASK_STACKSIZE];
OS_STK    TaskDraw3D3_stk[TASK_STACKSIZE];

/* Definition of Task Priorities */
#define TaskBenchmark_PRIORITY      1
#define TaskStartUp_PRIORITY        2
#define TaskSerialInput_PRIORITY    3
#define TaskFPSCounter_PRIORITY     4
#define TaskDraw3D_PRIORITY         5
#define VALUES 2600            //the maximum value the array can contain. (240/6 == 40 Lines in total)

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
int benchmarkIsRunning = 0;
int benchMarkState = 0;
int totalTimeRunning = 0;
int totalFps = 0;
int minFps = 10000;
int maxFps = 0;
int avgFps = 0;
int stopTask3D = 0;
int objectID = 0;
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
	volatile short * pixel_buffer = (short *) 0x08000000;// VGA pixel buffer

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
void resetScreen(int removeText){

	VGA_box (0, 0, 319, 239, 0);
	if(removeText){
		int i;
		for(i = 0; i < 80; i++){
			int j;
			for(j = 0; j < 60; j++){
				VGA_text (i, j, " ");
			}
		}
	}

}

//draws a pixel on the screen
void drawPixel(int x, int y, int size, short pixel_color )
{

	volatile short * pixel_buffer = (short *) 0x08000000;
	if(x != 0){
		int offset;
		offset = (y << 9) + x;
		*(pixel_buffer + offset) = pixel_color;
	}
}

void DrawLineV2(float x1, float y1, float x2, float y2, float midX, float midY, int size, int remove, short pixel_color)
{
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

	//calculate the position of x1 based on the midpoint of the screen.
    x1 = x1 + midX;
	x2 = x2 + midX;
	y1 = y1 + midY;
	y2 = y2 + midY;

	//calculates how many y has changed when x changed 1. pixelSteps is what y has to change every step.
    float pixelSteps;
    if(x1 == x2){
    	pixelSteps = ((x2 - x1) / (y2 - y1));
    } else {
    	pixelSteps = ((y2 - y1) / (x2 - x1));
    }

    //setup vars for loop
    float x = 0;
    float y = y1;
    float prevY = y1;
	volatile short * pixel_buffer = (short *) 0x08000000;

	int offset;
	//pixelSteps has to have a maximum because otherwise it will crash.
    if(pixelSteps < 1000 && pixelSteps > -1000){
    	 for(x = x1; x <= x2; x++){
    	 	//draws a pixel on every y between y1 and y2 if x1 equals x2
			if(x1 == x2){
				for(y = y1; y <= y2; y++){
					if(!offscreen){
						if (!remove)
						{
							if(x != 0){
								offset = ((int)y << 9) + (int)x;
								*(pixel_buffer + offset) = pixel_color;
							}
						}
						else
						{
							offset = ((int)y << 9) + (int)x;
							*(pixel_buffer + offset) = 0x0000;
						}
					}
				}
			} else {
				//draws pixels on every x between x1 and x2, y is calculated by how many times x has looped.
				if(!offscreen){
					if (!remove)
					{
						if(x != 0){
							offset = ((int)y << 9) + (int)x;
							*(pixel_buffer + offset) = pixel_color;
						}
					}
					else
					{
						offset = ((int)y << 9) + (int)x;
						*(pixel_buffer + offset) = 0x0000;
					}
				}
				y = (pixelSteps * (x - x1)) + y1;

				//this fucntion is for if pixelstep is bigger than 1 or smaller than -1.
				//Because it will not draw on every y position a pixel you have to fill it with pixels between the previous y and the current y.
				float tempY;
				if((int)pixelSteps > 1){
					for(tempY = 0; tempY <= pixelSteps; tempY++){
						if(!offscreen){
							if (!remove)
							{
								if(x != 0 && (y + tempY) <= y2){
									offset = ((int)y + (int)tempY << 9) + ((int)x + 1);
									*(pixel_buffer + offset) = pixel_color;
								}
							}
							else
							{
								offset = ((int)y + (int)tempY << 9) + ((int)x + 1);
								*(pixel_buffer + offset) = 0x0000;
							}
						}
					}
				} else if ((int)pixelSteps < -1){
					for(tempY = 0; tempY >= pixelSteps; tempY--){
						if(!offscreen){
							if (!remove)
							{
								if(x != 0 && (y + tempY) >= y2){
									offset = ((int)y + (int)tempY << 9) + ((int)x + 1);
									*(pixel_buffer + offset) = pixel_color;
								}
							}
							else
							{
								offset = ((int)y + (int)tempY << 9) + ((int)x + 1);
								*(pixel_buffer + offset) = 0x0000;
							}
						}
					}
				}
			}
			prevY = y;
		}
    }
}
//creates the task already because the tasks have to be started in the serial Input task.
void TaskBenchmark(void* pdata);
void TaskDraw3D(void* pdata);
//Task for sending information from the simulation/editor to the FPGA. Also the task to change rotation and zoom by using the switches.
void TaskSerialInput(void* pdata)
{

    //value of current switches enabled
    int SWITCH_value;

    //fast upload bool
    int ignoreDelayWhileUpload = 0;


    //pointers for accessing data from other hardware
	volatile int* UART_DATA_ptr = (int*) 0x10001010;
	volatile int* UART_CONTROL_ptr = (int *) 0x10001014;
	volatile int * SWITCH_ptr = (int *) 0x10000040;
	volatile short * sd_ram = (short *) 0x00000000;
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

	int singleObjectTaskCreated = 0;
	int multipleObjectTaskCreated = 0;

	//contains data received from serial
	int dataSerial;
    while (1)
    {
    		printf("Start task Serial Input \n");

  		while(1){
			SWITCH_value = *(SWITCH_ptr);
	  		ALT_SEM_PEND(sem_objectDrawn, 0);
	  		dataSerial = *(UART_DATA_ptr);

	  		if(!benchmarkIsRunning){
		  		if(dataSerial & 0x8000){
		  			if(stopDrawing){        //only run this if the Taskdraw3D is done with drawing.
		  				int pixelPos = (int)((totalAmountOfCharactersReceived * oneProcentOfCharactersToBeSend) * loadingBarOneProcent);
		  				int x = 0;
		  				for(x = 0; x < (int)pixelPos; x++){
		  					drawPixel(20 + x,162, 7, 0x0F00);
		  					char text_bottom_row1[40];
							sprintf(text_bottom_row1,  "%d %c",(int)(totalAmountOfCharactersReceived * oneProcentOfCharactersToBeSend), '%');
							VGA_text (35, 41, text_bottom_row1);
		  				}

		  			}

		  			char customString[2] = {(char)dataSerial, '\0'};
		  			//if the FPGA receives a 't' he knows how many characters will come.
		  			//printf("RECEIVED: %c\n", customString[0]);
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

		  				resetScreen(1);
		  				int i;

		  				char onScreenTitle = "NEON 3D Inc.";
						VGA_text(1,0,onScreenTitle);
		  				for(i = 0; i < VALUES; i++){
		  					//printf("reset index: %d \n", i);
		  					*(sd_ram + (i + (1 << 18))) = 0;
		  				}
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

						ignoreDelayWhileUpload = 1;

						VGA_text(31, 30, "LOADING OBJECT");
					//if the FPGA receives a 'd' or a ' ' he knows that he received all characters and the taskDraw3D can start drawing the object.
		  			} else if(customString[0] == 'd' || customString[0] == ' '){
		  				resetScreen(1);

		  				char onScreenTitle[40] = "NEON 3D Inc.";
		  				VGA_text(1,0,onScreenTitle);
		  				int amountOfLines = amountOfIntegersReceived / 6;
						char text_bottom_row1[40];
						sprintf(text_bottom_row1, "3D amount of lines: %d", amountOfLines );
						VGA_text (1, 1, text_bottom_row1);

						totalAmountOfCharactersToBeSend = 0;
						totalAmountOfCharactersReceived = 0;
						oneProcentOfCharactersToBeSend = 0;
						loadingBarOneProcent = 2.4;


		  				stopDrawing = 0;
		  				ignoreDelayWhileUpload = 0;
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

							*(sd_ram + (amountOfIntegersReceived + (1 << 18))) = total;
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
							*(sd_ram + (amountOfIntegersReceived + (1 << 18))) = total;

							amountOfIntegersReceived++;

							int amountOfLines = amountOfIntegersReceived / 6;
							char onScreenAmountOfLines[40];
							sprintf(onScreenAmountOfLines, "3D amount of lines: %d", amountOfLines );
							VGA_text (1, 1, onScreenAmountOfLines);
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
						if(yRotation < 0){
							yRotation = 359;
						}
						rotation[1] = yRotation;
	                //if the PGA recives a 'l' x rotation will decrease with 1.
		  			} else if(customString[0] == 'l'){
		  				xRotation = xRotation - 1;
						if(xRotation < 0){
							xRotation = 359;
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
						if(zRotation < 0){
							zRotation = 359;
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
						if(yRotation < 0){
							yRotation = 359;
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
						if(xRotation  < 0){
							xRotation = 359;
						}
						rotation[0] = xRotation;
					}
	                //switch 5 decreases z rotation with 1.
					if (SWITCH_value & 0x20)
					{
						zRotation = zRotation - 1;
						if(zRotation < 0){
							zRotation = 359;
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
						OSTaskCreateExt(TaskBenchmark,
						                  NULL,
						                  (void *)&TaskBenchmark_stk[TASK_STACKSIZE-1],
						                  TaskBenchmark_PRIORITY,
						                  TaskBenchmark_PRIORITY,
						                  TaskBenchmark_stk,
						                  TASK_STACKSIZE,
						                  NULL,
						                  0);
					} else {
						VGA_text(31, 29, "                     ");
						offscreen = 0;
					}
					//switch 8 will switch between multiple objects or single object
					if (SWITCH_value & 0x100)
					{



						if(!multipleObjectTaskCreated){

							int midPoint2[3] = {80, 120,1};
							int midPoint3[3] = {240,120,2};
							resetScreen(0);
							stopTask3D = 1;
							OSTimeDly(100);
							OSTaskCreateExt(TaskDraw3D,
												  midPoint2,
												  (void *)&TaskDraw3D2_stk[TASK_STACKSIZE-1],
												  TaskDraw3D_PRIORITY,
												  TaskDraw3D_PRIORITY,
												  TaskDraw3D2_stk,
												  TASK_STACKSIZE,
												  NULL,
												  0);
							OSTaskCreateExt(TaskDraw3D,
											  midPoint3,
											  (void *)&TaskDraw3D3_stk[TASK_STACKSIZE-1],
											  TaskDraw3D_PRIORITY + 1,
											  TaskDraw3D_PRIORITY + 1,
											  TaskDraw3D3_stk,
											  TASK_STACKSIZE,
											  NULL,
											  0);
							multipleObjectTaskCreated = 1;
							singleObjectTaskCreated = 0;
						}
					}
					else {



						if(!singleObjectTaskCreated){
							int midPoint1[3] = {160, 120, 0};
							resetScreen(0);
							stopTask3D = 1;
							OSTimeDly(500);
							printf("Starting sinlge task \n");
							OSTaskCreateExt(TaskDraw3D,
								  midPoint1,
								  (void *)&TaskDraw3D_stk[TASK_STACKSIZE-1],
								  TaskDraw3D_PRIORITY,
								  TaskDraw3D_PRIORITY,
								  TaskDraw3D_stk,
								  TASK_STACKSIZE,
								  NULL,
								  0);

							singleObjectTaskCreated = 1;
							multipleObjectTaskCreated = 0;
						}
					}

					//switch 13 and switch 14 are both high
					if(SWITCH_value & 0x02000 && SWITCH_value & 0x04000){
						objectID = 0;
					}else {
						objectID = 0;
					}
					//switch 13 rotation with object id 2 will rotate
					if(SWITCH_value & 0x02000){
						objectID = 2;
					}
					//switch 14 rotation with object id 1 will rotate
					if(SWITCH_value & 0x04000){
						objectID = 1;
					}
					//switch 18 decreases zoom with 0.01
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

	  		if(!ignoreDelayWhileUpload){
		  		OSTimeDly(15);
	  		} else {
	  			OSTimeDly(1);
	  		}
	  	}
	}
}


//Reads the startEndNodes array and performs the rotation calculations on it while pushing the caclulated values to the drawLine function.
void TaskDraw3D(void* pdata)
{

	volatile short * sd_ram = (short *) 0x00000000;
	int x1FirstNode = *(sd_ram + (1+ (1 << 18)));

			printf("first node x1: %d", x1FirstNode);

	printf("Start task draw 3D");
	stopTask3D = 0;
    //to make sure nothing is on the screen before drawing starts.
    resetScreen(1);

	//default line color
	short color = 0x0F00;

	//midpoint of screen
	//int midX = (320/2);
	//int midY = (240/2);

	int *midPoint = pdata;
	int midX = midPoint[0];
	int midY = midPoint[1];

	stopTask3D = 0;
	if(midPoint[2] == 1 ){
		OSTimeDly(500);
	}

	//for erasing previous rotated/zoomed object
	float prevousZoomScreenBR = 0;
	int previousXRotation = 0;
	int previousYRotation = 0;
	int previousZRotation = 0;

	char onScreenTitle[40] = "NEON 3D Inc.";
	char onScreenAmountOfLines[40];
	sprintf(onScreenAmountOfLines, "3D amount of lines: %d", 24);

	VGA_text(1,0, onScreenTitle);
	VGA_text (1, 1, onScreenAmountOfLines);


    //calculate all sin/cos values for given rotation
	float sinArray[360];
	float cosArray[360];

	int i;
	for(i = 0; i < 360; i++){

		sinArray[i] = sin(i / (180 / M_PI));
		cosArray[i] = cos(i / (180 / M_PI));
	}

	//local array to perform rotation and zoom calculations on
	int temporaryStartEndNodes[VALUES];

	while(!stopTask3D){
	    //wait for other task to stop accessing global values such as rotation, zoom and startendnodes array
		ALT_SEM_PEND(sem_objectDrawn, 0);

        //local start and end node values
		float startx = 0;
		float starty = 0;
		float startz = 0;
		float endx = 0;
		float endy = 0;
		float endz = 0;


		if(objectID  == midPoint[2] || objectID == 0){
        //local rotation values
			int localRotationX = rotation[0];
			int localRotationY = rotation[1];
			int localRotationZ = rotation[2];
			float zoomscreen = *zoomscreenpointer;

			//check if it should stop drawing/stop performing calculations on array (set in TaskSerialInput)
			if(!stopDrawing){

				int linesDrawn;
				for (linesDrawn = 0; linesDrawn < VALUES/6; linesDrawn++)
				{
					//skips index every interation by 6
					int indexOfArray = linesDrawn * 6;

					//read start node from global array
					float x1 = startx = *(sd_ram + (indexOfArray + (1 << 18)));
					float y1 = starty = *(sd_ram + ((indexOfArray + 1)+ (1 << 18)));
					float z1 = startz = *(sd_ram + ((indexOfArray + 2)+ (1 << 18)));

					//read end node from global array
					float x2 = endx = *(sd_ram + ((indexOfArray + 3)+ (1 << 18)));
					float y2 = endy = *(sd_ram + ((indexOfArray + 4)+ (1 << 18)));
					float z2 = endz = *(sd_ram + ((indexOfArray + 5)+ (1 << 18)));

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

						 DrawLineV2(x1,y1,x2,y2, midX, midY,1 , 1, color);
					}

				   //perform rotation calculations on the 3 dimensonal nodes and convert them to 2D nodes and then perform zoom calculation.
					 x1 = (((startx * cosArray[localRotationX] - (starty * cosArray[localRotationZ] - startz * sinArray[localRotationZ]) * sinArray[localRotationX]) * (cosArray[localRotationY])) + (starty * sinArray[localRotationZ] + startz * cosArray[localRotationZ]) * sinArray[localRotationY]) * zoomscreen;
					 x2 = (((endx * cosArray[localRotationX] - (endy * cosArray[localRotationZ] - endz * sinArray[localRotationZ]) * sinArray[localRotationX]) * (cosArray[localRotationY])) + (endy * sinArray[localRotationZ] + endz * cosArray[localRotationZ]) * sinArray[localRotationY]) * zoomscreen;

					 y1 = (((-(startx * cosArray[localRotationX] - (starty * cosArray[localRotationZ] - startz * sinArray[localRotationZ]) * sinArray[localRotationX])) * (sinArray[localRotationY]) + (starty * sinArray[localRotationZ] + startz * cosArray[localRotationZ]) * cosArray[localRotationY]))* zoomscreen;
					 y2 = (((-(endx * cosArray[localRotationX] - (endy * cosArray[localRotationZ] - endz * sinArray[localRotationZ]) * sinArray[localRotationX])) * (sinArray[localRotationY]) + (endy * sinArray[localRotationZ] + endz * cosArray[localRotationZ]) * cosArray[localRotationY]))* zoomscreen;

					//if offscreen benchmark mode is enabled, do not execute draw functions (which is the current bottleneck)

					DrawLineV2(x1,y1,x2,y2, midX, midY, 1 , 0, color);
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

				//OSTimeDly(1);


			}
        //other tasks may perform actions on global values used in this task
		}
		OSTimeDly(1);
		ALT_SEM_POST(sem_objectDrawn);
	}
	printf("Task removed");
	OSTaskDel(OS_PRIO_SELF);
}

//On startup, this task shows the Text Neon3D as a 3D object, and rotates it over the Y axle. It deletes itself after its finsished
void TaskStartUp(void* pdata){

	printf("Start StartUp task");
	volatile short * sd_ram = (short *) 0x00000000;
    //set startup values
	amountOfLines = 0;
	int rotatevalue[3] = {0,1,0};
	rotation = malloc(sizeof(int)*3);
	rotation = rotatevalue;

	float zoom = 0.8;
	zoomscreenpointer = malloc(sizeof(float));
	zoomscreenpointer = &zoom;

    //reset screen to remove any pixels or text thats not meant to be there
	resetScreen(1);

	stopDrawing = 0;

	char onScreenAmountOfLines[40];
	sprintf(onScreenAmountOfLines, "3D amount of lines: %d", 24 );
	VGA_text (1, 1, onScreenAmountOfLines);

	int test[150] = {-48,0,-2,-48,39,-1,-48,39,-1,-30,0,-2,-30,0,-2,-32,39,-1,-20,39,0,-19,-1,1,-8,38,-1,-20,39,0,-8,18,0,-21,18,0,-8,-1,-1,-19,-1,1,6,37,-1,6,-1,-1,25,-2,-1,6,-1,-1,25,-2,-1,25,37,0,6,37,-1,25,37,0,39,-1,-2,38,37,-1,38,37,-1,56,-1,-1,56,-1,-1,56,37,-1,-1,-53,-4,-24,-53,-1,-1,-53,-4,-1,-37,0,-1,-37,0,-23,-33,1,-1,-13,-1,-23,-12,1,-1,-37,0,-1,-13,-1,5,-55,1,20,-56,0,7,-36,-1,22,-35,-4,5,-55,1,7,-36,-1,22,-35,-4,20,-56,0,22,-13,-1,22,-35};

	int tempi;
	for(tempi = 1; tempi <= 150; tempi++){
		printf("WRITING TO SDRAM \n");
		*(sd_ram + (tempi+ (1 << 18))) = test[tempi-1];
		OSTimeDly(10);
	}


	//local rotation counter
	//OSTimeDly(1000);

	int zRotation;
	int xRotation;
	for(zRotation = 360; zRotation >= 270; zRotation--){
		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[2] = zRotation - 90;
		rotation[0] = xRotation;
		char onScreenZoom[40];
		char onScreenRotation[40];
		VGA_text (6, 2, "                                          \0");
		VGA_text (15, 3, "                                          \0");
		sprintf(onScreenZoom, "Zoom: %f", *zoomscreenpointer);
		VGA_text (1, 2, onScreenZoom);
		sprintf(onScreenRotation, "Rotation X,Y,Z: %d,%d,%d", rotation[0], rotation[1], rotation[2]);
		VGA_text (1, 3, onScreenRotation);
		xRotation++;
		ALT_SEM_POST(sem_objectDrawn);
		OSTimeDly(20);
	}

	OSTaskDel(OS_PRIO_SELF);
}

//this Task performs a benchmark to see the min FPS, max FPS and average FPS. It prints it in a table at the end of the Task
void TaskBenchmark(void* pdata){


	printf("BENCHMARK RUNS");
	//clear unremoved pixels from screen, but ignore text
	resetScreen(0);

	//tells other tasks that benchmark is running (halts TaskSerial)
	benchmarkIsRunning = 1;

	//default rotation value;
	int rotatevalue[3] = {0,0,0};

	//sets up character arrays for benchmark results later to be used in table
	char minOnscreen[10];
	char maxOnscreen[10];
	char avgOnscreen[10];
	char ttimeOnscreen[10];

	char minOffscreen[10];
	char maxOffscreen[10];
	char avgOffscreen[10];
	char ttimeOffscreen[10];

	ALT_SEM_PEND(sem_updateFps, 0);
	//resert all variables to it default values.
	//minFPS has to be high because otherwise it will never be set.
	totalFps = 0;
	maxFps = 0;
	minFps = 10000;
	totalTimeRunning = 0;
	avgFps = 0;

	ALT_SEM_POST(sem_updateFps);

	ALT_SEM_PEND(sem_objectDrawn, 0);
	//sets rotation and zoom to default value
	rotation = rotatevalue;
	*zoomscreenpointer = 0.8;
	ALT_SEM_POST(sem_objectDrawn);

	VGA_text(31, 1, "ONSCREEN BENCHMARK PASS: 1");
	int i;
	//state 1 x rotation to 50;
	for(i =0; i <= 50; i++){
		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[0] = i;
		ALT_SEM_POST(sem_objectDrawn);
		OSTimeDly(1);
	}

	ALT_SEM_PEND(sem_objectDrawn, 0);
	//sets rotation to default rotation value
	rotation = rotatevalue;

	ALT_SEM_POST(sem_objectDrawn);

	VGA_text(31, 1, "ONSCREEN BENCHMARK PASS: 2");

	//state 2 y rotation to 50;
	for(i =0; i <= 50; i++){
		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[1] = i;
		ALT_SEM_POST(sem_objectDrawn);
		OSTimeDly(1);
	}

	ALT_SEM_PEND(sem_objectDrawn, 0);
	//sets rotation to default rotation value
	rotation = rotatevalue;

	ALT_SEM_POST(sem_objectDrawn);

	VGA_text(31, 1, "ONSCREEN BENCHMARK PASS: 3");
	//state 3 z rotation to 50;
	for(i =0; i <= 50; i++){
		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[2] = i;
		ALT_SEM_POST(sem_objectDrawn);
		OSTimeDly(1);
	}

	ALT_SEM_PEND(sem_objectDrawn, 0);
	//sets rotation to default rotation value
	rotation = rotatevalue;

	ALT_SEM_POST(sem_objectDrawn);

	VGA_text(31, 1, "ONSCREEN BENCHMARK PASS: 4");
	//state 4 all axles to 50;
	for(i =0; i <= 50; i++){
		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[0] = i;
		rotation[1] = i;
		rotation[2] = i;
		ALT_SEM_POST(sem_objectDrawn);
		OSTimeDly(1);
	}

	ALT_SEM_PEND(sem_objectDrawn, 0);
	//resets rotation to default start value and sets zoom for last benchmark pass
	rotation = rotatevalue;
	*zoomscreenpointer = 0.1;

	ALT_SEM_POST(sem_objectDrawn);

	VGA_text(31, 1, "ONSCREEN BENCHMARK PASS: 5");
	//state 5 all axles to 50 and zoom;
	int addOrSubtract = 1;
	for(i =0; i < 360; i++){
		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[0] = i;
		rotation[1] = i;
		rotation[2] = i;

		if(*zoomscreenpointer >= 1.5){
			addOrSubtract = 0;
		} else if(*zoomscreenpointer <= 0.1){
			addOrSubtract = 1;
		}

		if(addOrSubtract){
			*zoomscreenpointer = *zoomscreenpointer + 0.01;
		} else {
			*zoomscreenpointer = *zoomscreenpointer - 0.01;
		}
		ALT_SEM_POST(sem_objectDrawn);
		OSTimeDly(1);
	}

	//allow fps task to run
	OSTimeDly(1);

	ALT_SEM_PEND(sem_objectDrawn, 0);
	//resets rotation and zoomto default start value
	rotation = rotatevalue;
	*zoomscreenpointer = 0.8;

	ALT_SEM_POST(sem_objectDrawn);

	//halfway through benchmark
	ALT_SEM_PEND(sem_updateFps, 0);;

	//puts currently calculated value for onscreen benchmark in character arrays
	sprintf(minOnscreen, "%d\0", minFps);
	sprintf(maxOnscreen, "%d\0", maxFps);
	sprintf(avgOnscreen, "%d\0", avgFps);
	sprintf(ttimeOnscreen, "%d\0", totalTimeRunning);

	//resets all values needed by benchmark and general fps calculations
	totalFps = 0;
	maxFps = 0;
	minFps = 10000;
	totalTimeRunning = 0;
	avgFps = 0;

	ALT_SEM_POST(sem_updateFps);

	//Tells DrawLineV2 that it cannot put calculated values in vga buffer
	offscreen = 1;

	ALT_SEM_PEND(sem_objectDrawn, 0);
	//sets rotation to default start value
	rotation = rotatevalue;

	ALT_SEM_POST(sem_objectDrawn);

	VGA_text(31, 1, "OFFSCREEN BENCHMARK PASS: 1");
	//state 1 x rotation to 50;
	for(i =0; i <= 50; i++){
		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[0] = i;
		ALT_SEM_POST(sem_objectDrawn);
		OSTimeDly(1);
	}

	ALT_SEM_PEND(sem_objectDrawn, 0);
	//sets rotation to default start value
	rotation = rotatevalue;
	ALT_SEM_POST(sem_objectDrawn);

	VGA_text(31, 1, "OFFSCREEN BENCHMARK PASS: 2");
	//state 2 y rotation to 50;
	for(i =0; i <= 50; i++){
		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[1] = i;
		ALT_SEM_POST(sem_objectDrawn);
		OSTimeDly(1);
	}

	ALT_SEM_PEND(sem_objectDrawn, 0);
	//sets rotation to default start value
	rotation = rotatevalue;

	ALT_SEM_POST(sem_objectDrawn);

	VGA_text(31, 1, "OFFSCREEN BENCHMARK PASS: 3");
	//state 3 z rotation to 50;
	for(i =0; i <= 50; i++){
		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[2] = i;
		ALT_SEM_POST(sem_objectDrawn);
		OSTimeDly(1);
	}

	ALT_SEM_PEND(sem_objectDrawn, 0);
	//sets rotation to default start value
	rotation = rotatevalue;

	ALT_SEM_POST(sem_objectDrawn);

	VGA_text(31, 1, "OFFSCREEN BENCHMARK PASS: 4");
	//state 4 all axles to 50;
	for(i =0; i <= 50; i++){
		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[0] = i;
		rotation[1] = i;
		rotation[2] = i;
		ALT_SEM_POST(sem_objectDrawn);
		OSTimeDly(1);
	}

	ALT_SEM_PEND(sem_objectDrawn, 0);

	//resets rotation to default start value and sets zoom for last benchmark pass
	rotation = rotatevalue;
	*zoomscreenpointer = 0.1;

	ALT_SEM_POST(sem_objectDrawn);

	VGA_text(31, 1, "OFFSCREEN BENCHMARK PASS: 5");
	//state 5 turns all axles to 360 and zooms in and out between 1.5 and 0.1 zoom values per rotation with 0.01 steps;
	addOrSubtract = 1;
	for(i =0; i < 360; i++){
		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[0] = i;
		rotation[1] = i;
		rotation[2] = i;

		if(*zoomscreenpointer >= 1.5){
			addOrSubtract = 0;
		} else if(*zoomscreenpointer <= 0.1){
			addOrSubtract = 1;
		}

		if(addOrSubtract){
			*zoomscreenpointer = *zoomscreenpointer + 0.01;
		} else {
			*zoomscreenpointer = *zoomscreenpointer - 0.01;
		}
		ALT_SEM_POST(sem_objectDrawn);
		OSTimeDly(1);
	}

	ALT_SEM_PEND(sem_objectDrawn, 0);
	//resets rotation and zoom to its default values
	rotation = rotatevalue;
	*zoomscreenpointer = 0.8;

	ALT_SEM_POST(sem_objectDrawn);


	ALT_SEM_PEND(sem_updateFps, 0);

	sprintf(minOffscreen, "%d\0", minFps);
	sprintf(maxOffscreen, "%d\0", maxFps);
	sprintf(avgOffscreen, "%d\0", avgFps);
	sprintf(ttimeOffscreen, "%d\0", totalTimeRunning);

	VGA_text(45, 50, "----------------------------------");
	VGA_text(45, 51, "| SCR | AVG   | MIN | MAX | TIME |");
	VGA_text(45, 52, "--------------------------------");
	VGA_text(45, 53, "| ON  |       |     |     |      |");
	VGA_text(45, 54, "----------------------------------");
	VGA_text(53,53, avgOnscreen);
	VGA_text(61,53, minOnscreen);
	VGA_text(67,53, maxOnscreen);
	VGA_text(73,53, ttimeOnscreen);


	VGA_text(45, 55, "| OFF |       |     |     |      |");
	VGA_text(45, 56, "----------------------------------");
	VGA_text(53,55, avgOffscreen);
	VGA_text(61,55, minOffscreen);
	VGA_text(67,55, maxOffscreen);
	VGA_text(73,55, ttimeOffscreen);
	VGA_text(31, 1, "                                         ");

	//resets all values needed by benchmark and general fps calculations
	totalFps = 0;
	maxFps = 0;
	minFps = 10000;
	totalTimeRunning = 0;
	avgFps = 0;

	//tells other tasks that benchmark is not running anymore, so they can proceed with their normal duties
	benchmarkIsRunning = 0;

	//needed tot tell DrawLineV2 that it may acces the vga buffer again
	offscreen = 0;

	//removes text for benchmark and any unremoved lines, but doessnt reset text
	VGA_text(31, 1, "                                   ");
	resetScreen(0);

	ALT_SEM_POST(sem_updateFps);

	OSTaskDel(OS_PRIO_SELF);
}
//Prints every second the value of fpsCounter (which is incremented in TaskDraw3D) and resets it.
void TaskFPSCounter(void* pdata){

	printf("FPS COUNTER RUNS");
	while(1){
		ALT_SEM_PEND(sem_updateFps, 0);
		//updates maximum fps
		if(fpsCounter > maxFps){
			maxFps = fpsCounter;
		}

		//updates minimum fps
		if(fpsCounter < minFps){
			minFps = fpsCounter;
		}

		//show avergae fps during benchmark, calculated by totalFps/totalTimerunning
		if(benchmarkIsRunning){

			totalTimeRunning++;
			totalFps = totalFps + fpsCounter;

			avgFps = totalFps / totalTimeRunning;

			char onScreenAvgFps[50];
			sprintf(onScreenAvgFps, "AVG FPS: %d", avgFps);
			VGA_text (1, 59, "                                     ");
			VGA_text (1, 59, onScreenAvgFps);
		}



		char onScreenFps[40];
		char onScreenMinFps[40];
		char onScreenMaxFps[40];
		sprintf(onScreenFps, "CURRENT FPS: %d", fpsCounter);
		sprintf(onScreenMinFps, "MIN FPS: %d", minFps);
		sprintf(onScreenMaxFps, "MAX FPS: %d", maxFps);
		VGA_text (1, 56, "                                     ");
		VGA_text (1, 56, onScreenFps);
		VGA_text (1, 57, "                                     ");
		VGA_text (1, 57, onScreenMinFps);
		VGA_text (1, 58, "                                     ");
		VGA_text (1, 58, onScreenMaxFps);
		fpsCounter = 0;

		ALT_SEM_POST(sem_updateFps);
		OSTimeDly(1000);

	}
}

/* The main function creates two task and starts multi-tasking */
int main(void)
{
	int midPoint1[2] = {160, 120};
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
	/*
	OSTaskCreateExt(TaskDraw3D,
                  midPoint1,
                  (void *)&TaskDraw3D_stk[TASK_STACKSIZE-1],
                  TaskDraw3D_PRIORITY,
                  TaskDraw3D_PRIORITY,
                  TaskDraw3D_stk,
                  TASK_STACKSIZE,
                  NULL,
                  0); */
	//second object

	//default start screen
	OSTaskCreateExt(TaskStartUp,
	                  NULL,
	                  (void *)&TaskStartUp_stk[TASK_STACKSIZE-1],
	                  TaskStartUp_PRIORITY,
	                  TaskStartUp_PRIORITY,
	                  TaskStartUp_stk,
	                  TASK_STACKSIZE,
	                  NULL,
	                  0);

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
