#include <stdio.h>
#include "includes.h"
#include <os/alt_sem.h>


/* Definition of Task Stacks */
#define   TASK_STACKSIZE       4096             //we need such a big stacksize because we save sin(0-360) and cos(0-360) in there to get a better performance.

OS_STK    TaskShowCredits_stk[TASK_STACKSIZE];
OS_STK    TaskSerialInput_stk[TASK_STACKSIZE];
OS_STK    TaskStartUp_stk[TASK_STACKSIZE];
OS_STK    TaskBenchmark_stk[TASK_STACKSIZE];
OS_STK    TaskFPSCounter_stk[TASK_STACKSIZE];
OS_STK    TaskDraw3D_stk[TASK_STACKSIZE];
OS_STK    TaskDraw3D2_stk[TASK_STACKSIZE];
OS_STK    TaskDraw3D3_stk[TASK_STACKSIZE];

/* Definition of Task Priorities */
#define TaskShowCredits_PRIORITY    1
#define TaskBenchmark_PRIORITY      2
#define TaskStartUp_PRIORITY        3
#define TaskSerialInput_PRIORITY    4
#define TaskFPSCounter_PRIORITY     5
#define TaskDraw3D_PRIORITY         6

//the maximum of addresses to be used(2600/2/6 ~= 215 Lines in total)
#define VALUES 2600

//define pi constant
#define M_PI 3.14159265358979323846

//semaphores to prevent global values for corruption
ALT_SEM(sem_objectDrawn)
ALT_SEM(sem_updateFps)

//All pointers to hardware
volatile short * pixel_buffer = (short *) 0x08000000;		// Pointer to start of VGA pixel buffer
volatile char * character_buffer = (char *) 0x09000000;		// Pointer to start of VGA character buffer
volatile short * sd_ram = (short *) 0x00000000; 			// Pointer to start of SD Ram
volatile int* UART_DATA_ptr = (int*) 0x10001010; 			// Pointer to UART Data buffer
volatile int* UART_CONTROL_ptr = (int *) 0x10001014; 		// Pointer to UART CONTROL
volatile int * SWITCH_ptr = (int *) 0x10000040;				// Pointer to start of SWITCH data

//globale variable of the object array, zoom, rotation, amount of lines, fpscounter.
float *zoomscreenpointer;
int *rotation;
int amountOfLines;

//Framerate Values shared by benchmark task and fps counter task
int fpsCounter  = 0;
int totalTimeRunning = 0;
int totalFps = 0;
int minFps = 10000;
int maxFps = 0;
int avgFps = 0;

//Cross Task Control
int objectID = 0;
int stopDrawing = 1;        //we made this variable because you can't change the rotation and zoom if the object isn't fully drawn yet.
int offscreen   = 0;        //we made this variable so we could stop the function drawLine to draw lines. We did this to check the performance.
int benchmarkIsRunning = 0;
int stopTask3D = 0;

/* FUNCTIONS */

//Write text on the screen.
void VGA_text(int x, int y, char * text_ptr)
{
	int offset;

	/* assume that the text string fits on one line */
	offset = (y << 7) + x;
	while ( *(text_ptr) )
	{
		*(character_buffer + offset) = *(text_ptr);	// write to the character buffer
		++text_ptr;
		++offset;
	}
}

//reset screen to a black screen. Removes all boxes and text.
void resetScreen(int removeText)
{

	int offset, row, col;

	/* assume that the box coordinates are valid */
	for (row = 0; row <= 239; row++)
	{
		col = 0;
		while (col <= 319)
		{
			offset = (row << 9) + col;
			*(pixel_buffer + offset) = 0x0000;	// compute halfword address, set pixel
			++col;
		}
	}
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
	int offset;
	offset = (y << 9) + x;
	*(pixel_buffer + offset) = pixel_color;
}

//draws a line between 2 points on screen, starting from the middle of the screen
void DrawLineV2(float x1, float y1, float x2, float y2, float midX, float midY, int remove, short pixel_color)
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

	int offset;

	//pixelSteps has to have a maximum because otherwise it will crash.
    if(pixelSteps < 200 && pixelSteps > -200){
    	 for(x = x1; x <= x2; x++){
    	 	//draws a pixel on every y between y1 and y2 if x1 equals x2
			if(x1 == x2){
				for(y = y1; y <= y2; y++){
					if(!offscreen){
						if (!remove)
						{
								offset = ((int)y << 9) + (int)x;
								*(pixel_buffer + offset) = pixel_color;
						}
						else
						{
							offset = ((int)y << 9) + (int)x;
							*(pixel_buffer + offset) = 0x0000;
						}
					}
				}
			} else
			{
				//draws pixels on every x between x1 and x2, y is calculated by how many times x has looped.
				if(!offscreen){
					if (!remove)
					{
							offset = ((int)y << 9) + (int)x;
							*(pixel_buffer + offset) = pixel_color;
					}else
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
								if( (y + tempY) <= y2){
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
								if((y + tempY) >= y2){
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
		}
    }
}

//Array contains values 0-9 per index, combines values in array to single integer, depending how many single decimals(0-9) are in the array
int parseInteger(int ammountOfDecimals, char array[4]){
	int total;
	/*if(ammountOfDecimals  == 1){
		total = array[0];
	} else if(ammountOfDecimals == 2){
		total = (array[0] * 10 + array[1]);
	} else if(ammountOfDecimals == 3){
		total = (array[0] * 100 + array[1] * 10 + array[2]);
	} else if(ammountOfDecimals == 4){
		total = (array[0] * 1000 + array[1] * 100 + array[2] * 10 + array[3]);
	}else {
		total = 1;
	}*/
	int i;
	int total;
	for(i = 0; i < amountOfDecimals; i++){
	 	total = (10 ^ i) * integerArray[(amountOfDecimals - i)] + total;
	}
	return total;
}

/* TASKS */

//this task shows the credits screen of the 3D engine.
void TaskShowCredits(void* pdata){
	resetScreen(1);
	VGA_text(30, 29, "This 3D engine is made by:");
	VGA_text(30,30, "Eldin Zenderink & Willem Fikkert");
	VGA_text(30,31, "2 Juni 2016 ICTESA2B Windesheim");
	int i;
	for(i = 0; i < 2000000; i++);
	resetScreen(1);

	char onScreenAmountOfLines[40];
	sprintf(onScreenAmountOfLines, "3D amount of lines: %d", amountOfLines);
	VGA_text (1, 1, onScreenAmountOfLines);
	VGA_text(1,0, "Neon3D FPGA Engine");

	OSTaskDel(OS_PRIO_SELF);
}

//Reads the startEndNodes array and performs the rotation calculations on it while pushing the caclulated values to the drawLine function.
void TaskDraw3D(void* pdata)
{

	//remove any unremoved drawings by previous tasks
	resetScreen(0);

	//set final amount of lines before task starts drawing
	char onScreenAmountOfLines[40];
	sprintf(onScreenAmountOfLines, "3D amount of lines: %d", amountOfLines);
	VGA_text (1, 1, onScreenAmountOfLines);

	//contains taskData information and taskId
	int *taskData = pdata;
	int midX = taskData[0];
	int midY = taskData[1];
	int taskId = taskData[2];

	//two objects load properly and two objects delete properly
	if(taskData[2] == 1 || taskData[2] == 0 ){
		OSTimeDly(500);
	}
	stopTask3D = 0;

	//when in two objects mode, make first task start later to make the second task able to start, vice versa for deleting
	if(taskId == 1 || taskId == 0 ){
		OSTimeDly(500);
	}

	//make sure that when the task starts it doesn't delete itself
	stopTask3D = 0;

	//default line color: green
	short color = 0x0F00;

	//for erasing previous rotated/zoomed object
	float prevousZoomScreenBR = 0;
	int previousXRotation = 0;
	int previousYRotation = 0;
	int previousZRotation = 0;

	//default node start and node end values which will not change due to calculations
	float startx = 0;
	float starty = 0;
	float startz = 0;
	float endx = 0;
	float endy = 0;
	float endz = 0;

    //calculate all sin/cos values for given rotation
	float sinArray[360];
	float cosArray[360];

	int i;
	for(i = 0; i < 360; i++){

		sinArray[i] = sin(i / (180 / M_PI));
		cosArray[i] = cos(i / (180 / M_PI));
	}

	//max ammount of values to be read and default start address
	int ValuesofObject = VALUES/2;
	int startAddress = 0;

	while(!stopTask3D){
	    //wait for other task to stop accessing global values such as rotation, zoom and startendnodes array
		ALT_SEM_PEND(sem_objectDrawn, 0);

		//draws object depending on objectID which is set in TaskSerialInput by switch(13-14)
		if(objectID  == taskId || objectID == 0){

        	//local rotation and zoom values
			int localRotationX = rotation[0];
			int localRotationY = rotation[1];
			int localRotationZ = rotation[2];
			float zoomscreen = *zoomscreenpointer;

			//check if it should stop drawing/stop performing calculations on array (set in TaskSerialInput)
			if(!stopDrawing)
			{
				//set start index for reading from SDRAM depending which id the task has been given
				if(taskId == 1){
					ValuesofObject = VALUES/2;
					startAddress = 0;
				}else if(taskId == 2){
					ValuesofObject == VALUES/2;
					startAddress = VALUES/2;
				}else{
					startAddress = 0;
					ValuesofObject = VALUES;
				}

				int linesDrawn;
				for (linesDrawn = 0; linesDrawn < ValuesofObject/6; linesDrawn++)
				{
					//skips index every interation by 6
					int indexOfArray = linesDrawn * 6;

					//read start node from global array
					float x1 = startx = *(sd_ram + startAddress + (indexOfArray + (1 << 18)));
					float y1 = starty = *(sd_ram + startAddress + ((indexOfArray + 1)+ (1 << 18)));
					float z1 = startz = *(sd_ram + startAddress + ((indexOfArray + 2)+ (1 << 18)));

					//read end node from global array
					float x2 = endx = *(sd_ram + startAddress + ((indexOfArray + 3)+ (1 << 18)));
					float y2 = endy = *(sd_ram + startAddress + ((indexOfArray + 4)+ (1 << 18)));
					float z2 = endz = *(sd_ram + startAddress + ((indexOfArray + 5)+ (1 << 18)));

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

						 DrawLineV2(x1,y1,x2,y2, midX, midY, 1, color);
					}

					 //perform rotation calculations on the 3 dimensonal nodes and convert them to 2D nodes and then perform zoom calculation.
					 x1 = (((startx * cosArray[localRotationX] - (starty * cosArray[localRotationZ] - startz * sinArray[localRotationZ]) * sinArray[localRotationX]) * (cosArray[localRotationY])) + (starty * sinArray[localRotationZ] + startz * cosArray[localRotationZ]) * sinArray[localRotationY]) * zoomscreen;
					 x2 = (((endx * cosArray[localRotationX] - (endy * cosArray[localRotationZ] - endz * sinArray[localRotationZ]) * sinArray[localRotationX]) * (cosArray[localRotationY])) + (endy * sinArray[localRotationZ] + endz * cosArray[localRotationZ]) * sinArray[localRotationY]) * zoomscreen;

					 y1 = (((-(startx * cosArray[localRotationX] - (starty * cosArray[localRotationZ] - startz * sinArray[localRotationZ]) * sinArray[localRotationX])) * (sinArray[localRotationY]) + (starty * sinArray[localRotationZ] + startz * cosArray[localRotationZ]) * cosArray[localRotationY]))* zoomscreen;
					 y2 = (((-(endx * cosArray[localRotationX] - (endy * cosArray[localRotationZ] - endz * sinArray[localRotationZ]) * sinArray[localRotationX])) * (sinArray[localRotationY]) + (endy * sinArray[localRotationZ] + endz * cosArray[localRotationZ]) * cosArray[localRotationY]))* zoomscreen;

					DrawLineV2(x1,y1,x2,y2, midX, midY, 0, color);
				}

				//save previous zoomscreen if its different then before
				if (zoomscreen != prevousZoomScreenBR)
				{
					prevousZoomScreenBR = zoomscreen;
				}

				//remember previous rotation
				previousXRotation = localRotationX;
				previousYRotation = localRotationY;
				previousZRotation = localRotationZ;

				//update fps counter (after for loop is finished peforming calculation, a complete frame has been drawn)
				ALT_SEM_PEND(sem_updateFps, 0);
				fpsCounter++;
				ALT_SEM_POST(sem_updateFps);
			}
		}
		OSTimeDly(1);
		ALT_SEM_POST(sem_objectDrawn);
	}

	//remove any unremoved drawings by previous tasks
	resetScreen(0);

	OSTaskDel(OS_PRIO_SELF);
}

//On startup, this task shows the Text Neon3D as a 3D object, and rotates it over the Y axle. It deletes itself after its finsished
void TaskStartUp(void* pdata){

	 //reset screen to remove any pixels or text thats not meant to be there
	resetScreen(1);
	VGA_text(1,0, "Neon3D FPGA Engine");

    //set default start values
	amountOfLines = 24;

	int rotatevalue[3] = {0,1,0};
	rotation = malloc(sizeof(int)*3);
	rotation = rotatevalue;

	float zoom = 0.8;
	zoomscreenpointer = malloc(sizeof(float));
	zoomscreenpointer = &zoom;

	//default object
	int defaultObject[150] = {-48,0,-2,-48,39,-1,-48,39,-1,-30,0,-2,-30,0,-2,-32,39,-1,-20,39,0,-19,-1,1,-8,38,-1,-20,39,0,-8,18,0,-21,18,0,-8,-1,-1,-19,-1,1,6,37,-1,6,-1,-1,25,-2,-1,6,-1,-1,25,-2,-1,25,37,0,6,37,-1,25,37,0,39,-1,-2,38,37,-1,38,37,-1,56,-1,-1,56,-1,-1,56,37,-1,-1,-53,-4,-24,-53,-1,-1,-53,-4,-1,-37,0,-1,-37,0,-23,-33,1,-1,-13,-1,-23,-12,1,-1,-37,0,-1,-13,-1,5,-55,1,20,-56,0,7,-36,-1,22,-35,-4,5,-55,1,7,-36,-1,22,-35,-4,20,-56,0,22,-13,-1,22,-35};

	//put default object (text: Neon 3D) in memory
	int tempi;
	for(tempi = 1; tempi <= 150; tempi++){
		*(sd_ram + (tempi+ (1 << 18))) = defaultObject[tempi-1];
	}

	//tell TaskDraw3D to start drawing
	stopDrawing = 0;

	//give time for other tasks to settle
	OSTimeDly(1500);

	//local rotation counter
	int zRotation;
	int xRotation;
	for(zRotation = 360; zRotation >= 270; zRotation--){

		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[2] = zRotation - 90;
		rotation[0] = xRotation;
		xRotation++;
		ALT_SEM_POST(sem_objectDrawn);

		//update rotation text on screen
		char onScreenZoom[40];
		char onScreenRotation[40];
		sprintf(onScreenZoom, "Zoom: %f", *zoomscreenpointer);
		sprintf(onScreenRotation, "Rotation X,Y,Z: %d,%d,%d", rotation[0], rotation[1], rotation[2]);
		VGA_text (6, 2, "                                          \0");
		VGA_text (15, 3, "                                          \0");
		VGA_text (1, 2, onScreenZoom);
		VGA_text (1, 3, onScreenRotation);

		OSTimeDly(20);
	}
	//remove useless task
	OSTaskDel(OS_PRIO_SELF);
}

//this Task performs a benchmark to see the min FPS, max FPS and average FPS. It prints it in a table at the end of the Task
void TaskBenchmark(void* pdata){

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

	//resert all variables to it default values.
	//minFPS has to be high because otherwise it will never be set.
	ALT_SEM_PEND(sem_updateFps, 0);
	totalFps = 0;
	maxFps = 0;
	minFps = 10000;
	totalTimeRunning = 0;
	avgFps = 0;
	ALT_SEM_POST(sem_updateFps);

	//sets rotation and zoom to default value
	ALT_SEM_PEND(sem_objectDrawn, 0);
	rotation = rotatevalue;
	*zoomscreenpointer = 0.8;
	ALT_SEM_POST(sem_objectDrawn);

	VGA_text(31, 1, "ONSCREEN BENCHMARK PASS: 1");

	//state 1 x rotation to 50;
	int i;
	for(i =0; i <= 50; i++){
		ALT_SEM_PEND(sem_objectDrawn, 0);
		rotation[0] = i;
		ALT_SEM_POST(sem_objectDrawn);
		OSTimeDly(1);
	}

	//sets rotation to default rotation value
	ALT_SEM_PEND(sem_objectDrawn, 0);
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

	//sets rotation to default rotation value
	ALT_SEM_PEND(sem_objectDrawn, 0);
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

	//sets rotation to default rotation value
	ALT_SEM_PEND(sem_objectDrawn, 0);
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

	//resets rotation to default start value and sets zoom for last benchmark pass
	ALT_SEM_PEND(sem_objectDrawn, 0);
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

	//resets rotation and zoom to default start value
	ALT_SEM_PEND(sem_objectDrawn, 0);
	rotation = rotatevalue;
	*zoomscreenpointer = 0.8;
	ALT_SEM_POST(sem_objectDrawn);

	//halfway through benchmark, set onscreen benchmark recorded values in character arrays
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

	//sets rotation to default start value
	ALT_SEM_PEND(sem_objectDrawn, 0);
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

	//sets rotation to default start value
	ALT_SEM_PEND(sem_objectDrawn, 0);
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

	//sets rotation to default start value
	ALT_SEM_PEND(sem_objectDrawn, 0);
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

	//sets rotation to default start value
	ALT_SEM_PEND(sem_objectDrawn, 0);
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

	//resets rotation to default start value and sets zoom for last benchmark pass
	ALT_SEM_PEND(sem_objectDrawn, 0);
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

	//resets rotation and zoom to its default values
	ALT_SEM_PEND(sem_objectDrawn, 0);
	rotation = rotatevalue;
	*zoomscreenpointer = 0.8;
	ALT_SEM_POST(sem_objectDrawn);

	//print result of benchmark as table on screen
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
	VGA_text(45, 55, "| OFF |       |     |     |      |");
	VGA_text(45, 56, "----------------------------------");
	VGA_text(53,53, avgOnscreen);
	VGA_text(61,53, minOnscreen);
	VGA_text(67,53, maxOnscreen);
	VGA_text(73,53, ttimeOnscreen);
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

	ALT_SEM_POST(sem_updateFps);

	//tells other tasks that benchmark is not running anymore, so they can proceed with their normal duties
	benchmarkIsRunning = 0;

	//needed tot tell DrawLineV2 that it may access the vga buffer again
	offscreen = 0;

	//removes text for benchmark and any unremoved lines, but doessnt reset text
	VGA_text(31, 1, "                                   ");
	resetScreen(0);

	OSTaskDel(OS_PRIO_SELF);
}

//Prints every second the value of fpsCounter (which is incremented in TaskDraw3D) and resets it.
void TaskFPSCounter(void* pdata){

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

		//update fps values on screen
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

//Task for sending information from the simulation/editor to the FPGA. Also the task to change rotation and zoom by using the switches.
void TaskSerialInput(void* pdata)
{
    //value of current switches enabled
    int SWITCH_value;

    //pointers for accessing data from other hardware


	//contains received integer (to be combined)
	char integer[4];

	//contains data received from serial
	int dataSerial;

    //sets default values for serial task.
    int ignoreDelayWhileUpload = 0;

	int xRotation = rotation[0];
	int yRotation = rotation[1];
	int zRotation = rotation[2];
	float zoom = *zoomscreenpointer;

	int amountOfIntegersReceived = 0;
	float totalAmountOfCharactersToBeSend = 0;
	float totalAmountOfCharactersReceived = 0;
	float oneProcentOfCharactersToBeSend = 0;
	float loadingBarOneProcent = 2.4;

	int integerPosition = 0;

	int singleObjectTaskCreated = 0;
	int multipleObjectTaskCreated = 0;
	int creditsScreenIsRunning = 0;


    while (1)
    {
  		ALT_SEM_PEND(sem_objectDrawn, 0);
  		SWITCH_value = *(SWITCH_ptr);
  		dataSerial = *(UART_DATA_ptr);

  		if(!benchmarkIsRunning){
	  		if(dataSerial & 0x8000){

	  			//Draws progressbar for upload on scren
	  			if(stopDrawing){        //only run this if the Taskdraw3D is not drawing.
	  				int pixelPosition = (int)((totalAmountOfCharactersReceived * oneProcentOfCharactersToBeSend) * loadingBarOneProcent);
	  				int x = 0;
	  				for(x = 0; x < (int)pixelPosition; x++){
	  					drawPixel(20 + x,162, 7, 0x0F00);
	  					char text_bottom_row1[40];
						sprintf(text_bottom_row1,  "%d %c",(int)(totalAmountOfCharactersReceived * oneProcentOfCharactersToBeSend), '%');
						VGA_text (35, 41, text_bottom_row1);
	  				}
	  			}

	  			char characterReceived[2] = {(char)dataSerial, '\0'};

	  			//if the FPGA receives a 't' it knows how many characters will come.
	  			if(characterReceived[0] == 't'){
	  				if(integerPosition > 0){

						totalAmountOfCharactersToBeSend = 0;
						totalAmountOfCharactersReceived = 0;
						oneProcentOfCharactersToBeSend = 0;
						int total = parseInteger(integerPosition, integer);
						totalAmountOfCharactersToBeSend = total;
						oneProcentOfCharactersToBeSend = 100 / totalAmountOfCharactersToBeSend;

						integerPosition = 0;
					}
				//if the FPGA receives a 'i' then the FPGA knows which object has to change.
	  			}else if(characterReceived[0] == 'i'){
	  				objectID = parseInteger(integerPosition, integer);

	  				if(objectID == 3){
	  					amountOfIntegersReceived = 0;
						int i;
						for(i = 0; i < VALUES; i++){
							*(sd_ram + i + (1 << 18)) = 0;
						}
					} else if(objectID == 2){
						amountOfIntegersReceived = VALUES/2;
						int i;
						for(i = amountOfIntegersReceived ; i < VALUES; i++){
							*(sd_ram + i + (1 << 18)) = 0;
						}
					} else if(objectID == 1){
						amountOfIntegersReceived = 0;
						int i;
						for(i = 0 ; i < VALUES/2; i++){
							*(sd_ram + i + (1 << 18)) = 0;
						}
					} else {
						amountOfIntegersReceived = 0;
						int i;
						for(i = 0; i < VALUES; i++){
							*(sd_ram + i + (1 << 18)) = 0;
						}
					}

	  			//if the FPGA receives a 'n' then the FPGA knows that there will come a new object
	  			} else if(characterReceived[0] == 'n'){


	  				resetScreen(1);

					amountOfLines = 0;
					integerPosition = 0;

					xRotation = 0;
					rotation[0] = xRotation;
					yRotation = 0;
					rotation[1] = yRotation;
					zRotation = 0;
					rotation[2] = zRotation;

					*zoomscreenpointer = zoom;

					stopDrawing = 1;
					ignoreDelayWhileUpload = 1;

					char onScreenTitle = "NEON 3D Inc.";
					VGA_text(1,0,onScreenTitle);
					VGA_text(31, 30, "LOADING OBJECT");

				//if the FPGA receives a 'd' or a ' ' he knows that he received all characters and the taskDraw3D can start drawing the object.
	  			} else if(characterReceived[0] == 'd' || characterReceived[0] == ' '){
	  				resetScreen(1);

	  				//calculated the amount of lines of the object.
					int amountOfLines = amountOfIntegersReceived / 6;

	  				char onScreenTitle[40] = "NEON 3D Inc.";
	  				char onScreenAmountOfLines[40];
	  				sprintf(onScreenAmountOfLines, "3D amount of lines: %d", amountOfLines );
	  				VGA_text(1,0,onScreenTitle);
	  				VGA_text (1, 1, onScreenAmountOfLines);

					totalAmountOfCharactersToBeSend = 0;
					totalAmountOfCharactersReceived = 0;
					oneProcentOfCharactersToBeSend = 0;
					loadingBarOneProcent = 2.4;

	  				stopDrawing = 0;
	  				ignoreDelayWhileUpload = 0;

	  			//if the FPGA receives a 'm' he knows that he received a coordination which is lower than 0.
	  			} else if(characterReceived[0] == 'm'){

					if(integerPosition > 0){
						int total = parseInteger(integerPosition, integer) * -1;

						if(objectID != 0){
							*(sd_ram + (amountOfIntegersReceived + (1 << 18))) = total;
						} else {
							*(sd_ram + (amountOfIntegersReceived + (1 << 18))) = total;
							*(sd_ram + ((amountOfIntegersReceived + (VALUES/2)) + (1 << 18))) = total;
						}
						amountOfIntegersReceived++;

						int amountOfLines = amountOfIntegersReceived / 6;
						char text_bottom_row1[40];
						sprintf(text_bottom_row1, "3D amount of lines: %d", amountOfLines );
						VGA_text (1, 1, text_bottom_row1);

						integerPosition = 0;
					}
	  			}
	  			//if the FPGA receives a 'p' he knows that he received a coordination which is bigger than 0.
	  			else if(characterReceived[0] == 'p'){
	  				if(integerPosition > 0){

	  					int total = parseInteger(integerPosition, integer);
						if(objectID != 0){
							*(sd_ram + (amountOfIntegersReceived + (1 << 18))) = total;
						} else {
							*(sd_ram + (amountOfIntegersReceived + (1 << 18))) = total;
							*(sd_ram + ((amountOfIntegersReceived + (VALUES/2)) + (1 << 18))) = total;
						}

						amountOfIntegersReceived++;

						int amountOfLines = amountOfIntegersReceived / 6;
						char onScreenAmountOfLines[40];
						sprintf(onScreenAmountOfLines, "3D amount of lines: %d", amountOfLines );
						VGA_text (1, 1, onScreenAmountOfLines);
						integerPosition = 0;
					}
                //if the PGA recives a 'u' y rotation will increase with 1.
	  			} else if(characterReceived[0] == 'u'){
	  				yRotation = yRotation + 1;
					if(yRotation > 359){
						yRotation = 0;
					}
					rotation[1] = yRotation;
				//if the PGA recives a 'b' y rotation will decrease with 1.
	  			} else if(characterReceived[0] == 'b'){
	  				yRotation = yRotation - 1;
					if(yRotation < 0){
						yRotation = 359;
					}
					rotation[1] = yRotation;
                //if the PGA recives a 'l' x rotation will decrease with 1.
	  			} else if(characterReceived[0] == 'l'){
	  				xRotation = xRotation - 1;
					if(xRotation < 0){
						xRotation = 359;
					}
					rotation[0] = xRotation;
				//if the PGA recives a 'r' x rotation will increase with 1.
	  			} else if(characterReceived[0] == 'r'){
	  				xRotation = xRotation + 1;
					if(xRotation > 359){
						xRotation = 0;
					}
					rotation[0] = xRotation;
				//if the PGA recives a 'x' z rotation will decrease with 1.
	  			} else if(characterReceived[0] == 'x'){
					zRotation = zRotation - 1;
					if(zRotation < 0){
						zRotation = 359;
					}
					rotation[2] = zRotation;
				//if the PGA recives a 'z' z rotation will increase with 1.
				} else if(characterReceived[0] == 'z'){
					zRotation = zRotation + 1;
					if(zRotation > 359){
						zRotation = 0;
					}
					rotation[2] = zRotation;
			    //if the FPGA receives 's' rotation and zoom will be set to default values.
				} else if(characterReceived[0] == 's'){
					xRotation = 0;
					rotation[0] = xRotation;
	  				yRotation = 0;
					rotation[1] = yRotation;
					zRotation = 0;
					rotation[2] = zRotation;
					zoom = 0.8;
					*zoomscreenpointer = zoom;
	  			} else { 	//if the FPGA receives something else he knows it is a character between 0 and 9.
	  				integer[integerPosition] = characterReceived[0] - '0';
					integerPosition++;
	  			}

	  			//updates the received amount of characters needed to calculate progress upload
	  			if(stopDrawing){
		  			totalAmountOfCharactersReceived++;
	  			}

	  			//send receive confirmation back to the sender.
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

					//resets fps counter values
					ALT_SEM_PEND(sem_updateFps, 0);;

					totalFps = 0;
					maxFps = 0;
					minFps = 10000;
					totalTimeRunning = 0;
					avgFps = 0;

					ALT_SEM_POST(sem_updateFps);

					resetScreen(0);
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

                //switch 5 increases z rotation with 1.
				if (SWITCH_value & 0x20)
				{
					zRotation = zRotation + 1;
					if(zRotation > 359){
						zRotation = 0;
					}
					rotation[2] = zRotation;
				}

                //switch 6 decreases z rotation with 1.
				if (SWITCH_value & 0x40)
				{
					zRotation = zRotation - 1;
					if(zRotation < 0){
						zRotation = 359;
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
						  TaskDraw3D_PRIORITY + 1,
						  TaskDraw3D_PRIORITY + 1,
						  TaskDraw3D2_stk,
						  TASK_STACKSIZE,
						  NULL,
						  0);
						OSTaskCreateExt(TaskDraw3D,
						  midPoint3,
						  (void *)&TaskDraw3D3_stk[TASK_STACKSIZE-1],
						  TaskDraw3D_PRIORITY + 2,
						  TaskDraw3D_PRIORITY + 2,
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
						OSTimeDly(100);
						int midPoint1[3] = {160, 120, 0};
						resetScreen(0);
						stopTask3D = 1;
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

				//switch 13 rotation with object id 2 will rotate
				if(SWITCH_value & 0x02000){
					objectID = 2;
				}else if(SWITCH_value & 0x04000){		//switch 14 rotation with object id 1 will rotate
					objectID = 1;
				}else{
					//set objectID to zero for drawing all objects (if 2 objects are shown at the same time)
					objectID = 0;
				}
				
				//switch 15 decreases zoom with 0.01
				if(SWITCH_value & 0x08000){
					if(zoom >= 0.11){
						zoom = zoom - 0.01;
						*zoomscreenpointer = zoom;
					}
				}
				
                //switch 16 increases zoom with 0.01
				if(SWITCH_value & 0x10000){
					if(zoom <= 1.49){
						zoom = zoom + 0.01;
						*zoomscreenpointer = zoom;
					}
				}
				
				//switch 17 will show the credits screen
				if(SWITCH_value & 0x20000){
					OSTaskCreateExt(TaskShowCredits,
					  NULL,
					  (void *)&TaskShowCredits_stk[TASK_STACKSIZE-1],
					  TaskShowCredits_PRIORITY,
					  TaskShowCredits_PRIORITY,
					  TaskShowCredits_stk,
					  TASK_STACKSIZE,
					  NULL,
					  0);
				}
	  		}
  		}

        //set text on screen.
  		char onScreenZoom[40];
  		char onScreenRotation[40];
		sprintf(onScreenZoom, "Zoom: %f", *zoomscreenpointer);
		sprintf(onScreenRotation, "Rotation X,Y,Z: %d,%d,%d", rotation[0], rotation[1], rotation[2]);
		VGA_text (6, 2, "                                          \0");
		VGA_text (15, 3, "                                          \0");
		VGA_text (1, 2, onScreenZoom);
		VGA_text (1, 3, onScreenRotation);

  		ALT_SEM_POST(sem_objectDrawn);

  		if(!ignoreDelayWhileUpload){
	  		OSTimeDly(15);
  		} else {
  			OSTimeDly(1);
  		}
	}
}

/* The main function creates two task and starts multi-tasking */
int main(void)
{
	//reset sdram values on startup
	int i;
	for(i = 0; i < VALUES; i++){
		*(sd_ram + i + (1 << 18)) = 0;
	}

	//setup semaphores
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
