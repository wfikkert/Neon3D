
#include <stdio.h>
#include "includes.h"
#include <os/alt_sem.h>
/* Definition of Task Stacks */
#define   TASK_STACKSIZE       2048
OS_STK    task1_stk[TASK_STACKSIZE];
OS_STK    task2_stk[TASK_STACKSIZE];
OS_STK    task3_stk[TASK_STACKSIZE];

/* Definition of Task Priorities */

#define TASK1_PRIORITY      1
#define TASK2_PRIORITY      2
#define TASK3_PRIORITY      3
#define VALUES 1200

ALT_SEM(sem_objectDrawn)

int *startEndnodes;
int *rotation;
float *zoomscreenpointer;
int amountOfLines;
/****************************************************************************************
 * Subroutine to send a string of text to the VGA monitor
****************************************************************************************/
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

/****************************************************************************************
 * Draw a filled rectangle on the VGA monitor
****************************************************************************************/
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

void drawPixel(int x, int y, int size, short pixel_color )
{
	VGA_box (x, y, x + size - 1, y + size - 1, pixel_color);
}

void removePixel(int x, int y, int size)
{
	VGA_box (x, y, x +(size - 1), y + (size - 1), 0x0000);
}


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

        }

}

/* Prints "Hello World" and sleeps for three seconds */
void task1(void* pdata)
{
  int KEY_value;


	volatile int* UART_DATA_ptr = (int*) 0x10001010; // JTAG UART address
	volatile int* UART_CONTROL_ptr = (int *) 0x10001014;


	int dataSerial;
	int integerPos = 0;
	int amountOfIntegersReceived;
	char integer[4];

	int test[VALUES];
	startEndnodes = malloc(sizeof(int) * VALUES);
	startEndnodes = test;
  while (1)
  {
	  printf("Start task Serial Input");


		VGA_box (0, 0, 319, 239, 0);
	  	while(1){


	  		ALT_SEM_PEND(sem_objectDrawn, 0);
	  		dataSerial = *(UART_DATA_ptr);
	  		if(dataSerial & 0x8000){
	  			char customString[2] = {(char)dataSerial, '\0'};

	  			if(customString[0] == '-'){


					if(integerPos > 0){

						int total;
						if(integerPos == 1){
							total = integer[0] * -1;
						} else if(integerPos == 2){
							total = (integer[0] * 10 + integer[1]) *-1;
						} else if(integerPos == 3){
							total = (integer[0] * 100 + integer[1] * 10 + integer[2]) * -1;
						} else if(integerPos == 4){
							total = (integer[0] * 1000 + integer[1] * 100 + integer[2] * 10 + integer[3]) * -1;
						}
						test[amountOfIntegersReceived] = total;
						startEndnodes = test;

						amountOfIntegersReceived++;
					}

					char beforeDataString[40] = "+NODE RC: ";
					char customString[5] = {integer[0], integer[1], integer[2], integer[3], '\0'};
					VGA_text (1, 6, " ");
					VGA_text (1, 6, beforeDataString);
	  				VGA_text (14, 6, "                            ");
					VGA_text (14, 6, customString);
					integerPos = 0;
	  			}
	  			else if(customString[0] == '+'){
	  				if(integerPos > 0){

						int total;
						if(integerPos == 1){
							total = integer[0] * -1;
						} else if(integerPos == 2){
							total = (integer[0] * 10 + integer[1]);
						} else if(integerPos == 3){
							total = (integer[0] * 100 + integer[1] * 10 + integer[2]);
						} else if(integerPos == 4){
							total = (integer[0] * 1000 + integer[1] * 100 + integer[2] * 10 + integer[3]);
						}
						test[amountOfIntegersReceived] = total;
						startEndnodes = test;

						amountOfIntegersReceived++;
					}

	  				char beforeDataString[40] = "+NODE RC: ";
	  				char customString[5] = {integer[0], integer[1], integer[2], integer[3], '\0'};
	  				VGA_text (1, 6, " ");
	  				VGA_text (1, 6, beforeDataString);
	  				VGA_text (14, 6, "                            ");
					VGA_text (14, 6, customString);
					integerPos = 0;
	  			}
	  			else {
	  				integer[integerPos] = customString[0] + 0x30;
					char beforeDataString[40] = "SERIAL DATA:";
					VGA_text (1, 5, beforeDataString);
					VGA_text (14, 5, customString);
					integerPos++;
	  			}

	  		}

	  		volatile int * KEY_ptr = (int *) 0x10000040;
	  		KEY_value = *(KEY_ptr);

	  		int xRotation = rotation[0];
	  		int yRotation = rotation[1];
	  		float zoom = *zoomscreenpointer;


	  		if (KEY_value & 0x1)					// check KEY0
			{

				yRotation = 0;
				rotation[1] = yRotation;
				xRotation = 0;
				rotation[0] = xRotation;
				zoom = 0.8;
				*zoomscreenpointer = zoom;

				int test[VALUES];
				startEndnodes = test;
				amountOfLines = 0;

			}
	  		if (KEY_value & 0x2)					// check KEY0
	  		{
	  			yRotation = yRotation + 1;
	  			if(yRotation > 359){
	  				yRotation = 0;
	  			}
	  			rotation[1] = yRotation;

	  		}
	  		if (KEY_value & 0x4)					// check KEY01
	  		{
	  			yRotation = yRotation - 1;
	  			if(yRotation < 1){
	  				yRotation = 360;
	  			}
	  			rotation[1] = yRotation;

	  		}
	  		if (KEY_value & 0x8)					// check KEY02
	  		{
	  			xRotation = xRotation + 1;
	  			if(xRotation > 359){
	  				xRotation = 0;
	  			}
	  			rotation[0] = xRotation;

	  		}
	  		if (KEY_value & 0x10)					// check KEY03
	  		{
	  			xRotation = xRotation - 1;
	  			if(xRotation < 1){
	  				xRotation = 360;
	  			}
	  			rotation[0] = xRotation;

	  		}

	  		if(KEY_value & 0x20000){
	  			if(zoom < 1.5){
		  			zoom = zoom + 0.01;
		  			*zoomscreenpointer = zoom;
	  			}
	  		}

	  		if(KEY_value & 0x10000){
	  			if(zoom > 0.1){
					zoom = zoom - 0.01;
					*zoomscreenpointer = zoom;
	  			}
			}
	  		ALT_SEM_POST(sem_objectDrawn);
	  		OSTimeDlyHMSM(0,0,0,70);
	  	}
  }
}


/* Prints "Hello World" and sleeps for three seconds */
void task2(void* pdata)
{
	printf("Start task draw 3D #0");
	VGA_box (0, 0, 319, 239, 0);
	int midX = (320/2);
	int midY = (240/2);
	short color = 0x0F00;
	float prevousZoomScreenBR = 0;
	int previousXRotation = 0;
	int previousYRotation = 0;
	int i;
	int tempArray[VALUES];




	char text_top_row[40];
	char text_bottom_row[40];
	char text_bottom_row1[40];
	char text_bottom_row2[40];
	char text_bottom_row3[40] = "SOFTWARE MODE \0";
	sprintf(text_top_row, "NEON 3D Copyright W.Fikkert & E.Zenderink :D");

	VGA_text(1,0,text_top_row);
	VGA_text (1, 4, text_bottom_row3);

	while(1){

		OSTimeDlyHMSM(0,0,0,70);

		int linesDrawn;
		float startx = 0;
		float starty = 0;
		float startz = 0;
		float endx = 0;
		float endy = 0;
		float endz = 0;

		//ALT_SEM_PEND(sem_objectDrawn, 0);

		int localRotationX = rotation[0];
		int localRotationY = rotation[1];
		float zoomscreen = *zoomscreenpointer;
		VGA_text (1, 2, "                                          \0");
		VGA_text (1, 3, "                                          \0");

		sprintf(text_bottom_row, "Zoom: %f", zoomscreen);
		VGA_text (1, 2, text_bottom_row);
		sprintf(text_bottom_row2, "Rotation X,Y: %d,%d", rotation[0], rotation[1]);
		VGA_text (1, 3, text_bottom_row2);
		sprintf(text_bottom_row1, "3D amount of lines: %d", amountOfLines );
		VGA_text (1, 1, text_bottom_row1);
		for(i = 0;  i < VALUES; i++){
			tempArray[i] = (startEndnodes)[i];
		}
		startEndnodes = tempArray;

		for (linesDrawn = 0; linesDrawn < VALUES/6; linesDrawn++)
		{


			int indexOfArray = linesDrawn * 6;
			float x1 = startx = (startEndnodes)[indexOfArray];
			float y1 = starty = (startEndnodes)[indexOfArray + 1];
			float z1 = startz = (startEndnodes)[indexOfArray + 2];


			float x2 = endx = (startEndnodes)[indexOfArray + 3];
			float y2 = endy = (startEndnodes)[indexOfArray + 4];
			float z2 = endz = (startEndnodes)[indexOfArray + 5];


			if(x1 == 0 && x2 == 0 && y1 == 0 && y2 == 0 && z1 == 0 && z2 == 0){
				amountOfLines = indexOfArray - 1;
				break;
			}

			if(previousXRotation != localRotationX || previousYRotation != localRotationY || zoomscreen != prevousZoomScreenBR)
			{
				x1 = ((startx * cos(previousXRotation / 57.4) - starty * sin(previousXRotation / 57.4)) * (cos(previousYRotation / 57.4))) + startz * sin(previousYRotation / 57.4);
				x2 = ((endx * cos(previousXRotation / 57.4) - endy * sin(previousXRotation / 57.4)) * (cos(previousYRotation / 57.4))) + endz * sin(previousYRotation / 57.4);

				z1 = ((-(startx * cos(previousXRotation / 57.4) - starty * sin(previousXRotation / 57.4))) * (sin(previousYRotation / 57.4)) + startz * cos(previousYRotation / 57.4));
				z2 = ((-(endx * cos(previousXRotation / 57.4) - endy * sin(previousXRotation / 57.4))) * (sin(previousYRotation / 57.4)) + endz * cos(previousYRotation / 57.4));

				y1 = z1;
				y2 = z2;

				drawLine((x1 * prevousZoomScreenBR), (y1 * prevousZoomScreenBR), (x2 * prevousZoomScreenBR), (y2 * prevousZoomScreenBR), midX, midY, 1 , 1, 0x0000);

			}

			x1 = ((startx * cos(localRotationX / 57.4) - starty * sin(localRotationX / 57.4)) * (cos(localRotationY / 57.4))) + startz * sin(localRotationY / 57.4) ;
			x2 = ((endx * cos(localRotationX / 57.4) - endy * sin(localRotationX / 57.4))*(cos(localRotationY / 57.4))) + endz * sin(localRotationY / 57.4);

			z1 = ((-(startx * cos(localRotationX / 57.4) - starty * sin(localRotationX / 57.4))) *(sin(localRotationY / 57.4)) + startz * cos(localRotationY / 57.4));
			z2 = ((-(endx * cos(localRotationX / 57.4) - endy * sin(localRotationX / 57.4))) * (sin(localRotationY / 57.4)) + endz * cos(localRotationY / 57.4));



			y1 = z1;
			y2 = z2;
			drawLine((x1 * zoomscreen), (y1 * zoomscreen), (x2 * zoomscreen), (y2 * zoomscreen), midX, midY, 1 , 0, color);



		}

		if (zoomscreen != prevousZoomScreenBR)
		{
			prevousZoomScreenBR = zoomscreen;
		}

		previousXRotation = localRotationX;
		previousYRotation = localRotationY;

		//ALT_SEM_POST(sem_objectDrawn);

	}

}


void task3(void* pdata)
{
	printf("Start task draw 3D #1");
	VGA_box (0, 0, 319, 239, 0);
	int midX = 320/3;
	int midY = 240/3;
	short color = 0x0F00;
	float prevousZoomScreenBR = 0;
	int previousXRotation = 0;
	int previousYRotation = 0;
	int i;
	int tempArray[VALUES];




	char text_top_row[40];
	char text_bottom_row[40];
	char text_bottom_row1[40];
	char text_bottom_row2[40];
	char text_bottom_row3[40] = "SOFTWARE MODE \0";
	sprintf(text_top_row, "NEON 3D Copyright W.Fikkert & E.Zenderink :D");
	sprintf(text_bottom_row1, "3D amount of lines: %d", VALUES / 6);
	VGA_text(1,0,text_top_row);
	VGA_text (1, 1, text_bottom_row1);
	VGA_text (1, 4, text_bottom_row3);

	while(1){

		OSTimeDlyHMSM(0,0,0,70);

		int linesDrawn;
		float startx = 0;
		float starty = 0;
		float startz = 0;
		float endx = 0;
		float endy = 0;
		float endz = 0;

		ALT_SEM_PEND(sem_objectDrawn, 0);

		int localRotationX = rotation[0];
		int localRotationY = rotation[1];
		float zoomscreen = *zoomscreenpointer;
		VGA_text (1, 2, "                                          \0");
		VGA_text (1, 3, "                                          \0");

		sprintf(text_bottom_row, "Zoom: %f", zoomscreen);
		VGA_text (1, 2, text_bottom_row);
		sprintf(text_bottom_row2, "Rotation X,Y: %d,%d", rotation[0], rotation[1]);
		VGA_text (1, 3, text_bottom_row2);


		for(i = 0;  i < VALUES; i++){
			tempArray[i] = (startEndnodes)[i];
		}
		startEndnodes = tempArray;

		for (linesDrawn = 0; linesDrawn < VALUES/6; linesDrawn++)
		{


			int indexOfArray = linesDrawn * 6;
			float x1 = startx = (startEndnodes)[indexOfArray];
			float y1 = starty = (startEndnodes)[indexOfArray + 1];
			float z1 = startz = (startEndnodes)[indexOfArray + 2];


			float x2 = endx = (startEndnodes)[indexOfArray + 3];
			float y2 = endy = (startEndnodes)[indexOfArray + 4];
			float z2 = endz = (startEndnodes)[indexOfArray + 5];

			if(previousXRotation != localRotationX || previousYRotation != localRotationY || zoomscreen != prevousZoomScreenBR)
			{
				x1 = ((startx * cos(previousXRotation / 57.4) - starty * sin(previousXRotation / 57.4)) * (cos(previousYRotation / 57.4))) + startz * sin(previousYRotation / 57.4);
				x2 = ((endx * cos(previousXRotation / 57.4) - endy * sin(previousXRotation / 57.4)) * (cos(previousYRotation / 57.4))) + endz * sin(previousYRotation / 57.4);

				z1 = ((-(startx * cos(previousXRotation / 57.4) - starty * sin(previousXRotation / 57.4))) * (sin(previousYRotation / 57.4)) + startz * cos(previousYRotation / 57.4));
				z2 = ((-(endx * cos(previousXRotation / 57.4) - endy * sin(previousXRotation / 57.4))) * (sin(previousYRotation / 57.4)) + endz * cos(previousYRotation / 57.4));

				y1 = z1;
				y2 = z2;

				drawLine((x1 * prevousZoomScreenBR), (y1 * prevousZoomScreenBR), (x2 * prevousZoomScreenBR), (y2 * prevousZoomScreenBR), midX, midY, 1 , 1, 0x0000);

			}

			x1 = ((startx * cos(localRotationX / 57.4) - starty * sin(localRotationX / 57.4)) * (cos(localRotationY / 57.4))) + startz * sin(localRotationY / 57.4) ;
			x2 = ((endx * cos(localRotationX / 57.4) - endy * sin(localRotationX / 57.4))*(cos(localRotationY / 57.4))) + endz * sin(localRotationY / 57.4);

			z1 = ((-(startx * cos(localRotationX / 57.4) - starty * sin(localRotationX / 57.4))) *(sin(localRotationY / 57.4)) + startz * cos(localRotationY / 57.4));
			z2 = ((-(endx * cos(localRotationX / 57.4) - endy * sin(localRotationX / 57.4))) * (sin(localRotationY / 57.4)) + endz * cos(localRotationY / 57.4));



			y1 = z1;
			y2 = z2;
			drawLine((x1 * zoomscreen), (y1 * zoomscreen), (x2 * zoomscreen), (y2 * zoomscreen), midX, midY, 1 , 0, color);



		}

		if (zoomscreen != prevousZoomScreenBR)
		{
			prevousZoomScreenBR = zoomscreen;
		}

		previousXRotation = localRotationX;
		previousYRotation = localRotationY;

		ALT_SEM_POST(sem_objectDrawn);

	}

}


/* The main function creates two task and starts multi-tasking */
int main(void)
{
	amountOfLines = 0;
	int rotatevalue[2] = {0,0};
	rotation = malloc(sizeof(int)*2);
	rotation = rotatevalue;

	float zoom = 0.8;
	zoomscreenpointer = malloc(sizeof(float));
	zoomscreenpointer = &zoom;

	printf("\n array set \n");


	int err = ALT_SEM_CREATE(&sem_objectDrawn, 1);
  OSTaskCreateExt(task1,
                  NULL,
                  (void *)&task1_stk[TASK_STACKSIZE-1],
                  TASK1_PRIORITY,
                  TASK1_PRIORITY,
                  task1_stk,
                  TASK_STACKSIZE,
                  NULL,
                  0);


 /* OSTaskCreateExt(task2,
                  NULL,
                  (void *)&task2_stk[TASK_STACKSIZE-1],
                  TASK2_PRIORITY,
                  TASK2_PRIORITY,
                  task2_stk,
                  TASK_STACKSIZE,
                  NULL,
                  0);*/
  /*
  OSTaskCreateExt(task3,
                   NULL,
                   (void *)&task3_stk[TASK_STACKSIZE-1],
                   TASK3_PRIORITY,
                   TASK3_PRIORITY,
                   task3_stk,
                   TASK_STACKSIZE,
                   NULL,
                   0);
                   */
  OSStart();
  return 0;
}
