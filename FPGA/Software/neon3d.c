
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
#define VALUES 240

ALT_SEM(sem_objectDrawn)

int *startEndnodes;
int *rotation;
float *zoomscreenpointer;
int amountOfLines;
int stopDrawing = 1;
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

void drawPixel(int x, int y, int size, short pixel_color )
{
	if(x != 0){
		VGA_box (x, y, x + size - 1, y + size - 1, pixel_color);
	}
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

/* Prints "Hello World" and sleeps for three seconds */
void task1(void* pdata)
{
  int KEY_value;


	volatile int* UART_DATA_ptr = (int*) 0x10001010; // JTAG UART address
	volatile int* UART_CONTROL_ptr = (int *) 0x10001014;


	volatile int * KEY_ptr = (int *) 0x10000040;

	int xRotation = rotation[0];
	int yRotation = rotation[1];
	int zRotation = rotation[2];
	float zoom = *zoomscreenpointer;


	int dataSerial;
	int integerPos = 0;
	int amountOfIntegersReceived = 0;
	float totalAmountOfCharactersToBeSend = 0;
	float totalAmountOfCharactersReceived = 0;
	float oneProcentOfCharactersToBeSend = 0;
	float loadingBarOneProcent = 2.4;
	char integer[4];

	int test[VALUES];
	startEndnodes = malloc(sizeof(int) * VALUES);
	startEndnodes = test;
  while (1)
  {
	  printf("Start task Serial Input \n");


	  	while(1){


			KEY_value = *(KEY_ptr);
	  		ALT_SEM_PEND(sem_objectDrawn, 0);
	  		dataSerial = *(UART_DATA_ptr);
	  		if(dataSerial & 0x8000){

	  			if(stopDrawing){
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
	  			//printf("RECEIVED: %s \n", customString);
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
						//printf("INTEGER RECEIVED: %d \n", total);
						integerPos = 0;
					}
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
						//printf("INTEGER RECEIVED: %d \n", total);
						integerPos = 0;
					}

	  			}
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

						//printf("INTEGER RECEIVED: %d \n", total);
						integerPos = 0;
					}

	  			} else if(customString[0] == 'u'){
	  				yRotation = yRotation + 1;
					if(yRotation > 359){
						yRotation = 0;
					}
					rotation[1] = yRotation;
	  			} else if(customString[0] == 'b'){
	  				yRotation = yRotation - 1;
					if(yRotation < 1){
						yRotation = 360;
					}
					rotation[1] = yRotation;

	  			} else if(customString[0] == 'l'){
	  				xRotation = xRotation - 1;
					if(xRotation < 1){
						xRotation = 360;
					}
					rotation[0] = xRotation;
	  			} else if(customString[0] == 'r'){
	  				xRotation = xRotation + 1;
					if(xRotation > 359){
						xRotation = 0;
					}
					rotation[0] = xRotation;
	  			} else if(customString[0] == 'z'){
					zRotation = zRotation - 1;
					if(zRotation < 1){
						zRotation = 360;
					}
					rotation[2] = zRotation;
				} else if(customString[0] == 'x'){
					zRotation = zRotation + 1;
					if(zRotation > 359){
						zRotation = 0;
					}
					rotation[2] = zRotation;
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
	  			if (KEY_value & 0x1)					// check KEY0
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

				if (KEY_value & 0x20)					// check KEY04
				{
					zRotation = zRotation - 1;
					if(zRotation < 1){
						zRotation = 360;
					}
					rotation[2] = zRotation;

				}

				if (KEY_value & 0x40)					// check KEY04
				{
					zRotation = zRotation + 1;
					if(zRotation > 359){
						zRotation = 0;
					}
					rotation[2] = zRotation;

				}

				if(KEY_value & 0x20000){
					if(zoom <= 1.49){
						zoom = zoom + 0.01;
						*zoomscreenpointer = zoom;
					}
				}

				if(KEY_value & 0x10000){
					if(zoom >= 0.11){
						zoom = zoom - 0.01;
						*zoomscreenpointer = zoom;
					}
				}
	  		}


	  		char text_bottom_row[40];
	  		char text_bottom_row2[40];
			VGA_text (6, 2, "                                          \0");
			VGA_text (15, 3, "                                          \0");
			sprintf(text_bottom_row, "Zoom: %f", *zoomscreenpointer);
			VGA_text (1, 2, text_bottom_row);
			sprintf(text_bottom_row2, "Rotation X,Y,Z: %d,%d,%d", rotation[0], rotation[1], rotation[2]);
			VGA_text (1, 3, text_bottom_row2);

	  		ALT_SEM_POST(sem_objectDrawn);
	  		OSTimeDly(1);
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
	int previousZRotation = 0;
	int i;
	int tempArray[VALUES];




	char text_top_row[40] = "NEON 3D Inc.";

	VGA_text(1,0,text_top_row);
	char text_bottom_row1[40];
	sprintf(text_bottom_row1, "3D amount of lines: %d", 0);
	VGA_text (1, 1, text_bottom_row1);


	char text_bottom_row3[40] = "SOFTWARE MODE \0";
	VGA_text (56, 0, text_bottom_row3);

	while(1){

		//OSTimeDlyHMSM(0,0,0,70);

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
		int localRotationZ = rotation[2];
		float zoomscreen = *zoomscreenpointer;

		for(i = 0;  i < VALUES; i++){
			tempArray[i] = (startEndnodes)[i];
		}
		startEndnodes = tempArray;

		if(!stopDrawing){
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
					break;
				}

				if(previousXRotation != localRotationX || previousYRotation != localRotationY || previousZRotation != localRotationZ || zoomscreen != prevousZoomScreenBR)
				{
					 x1 = ((startx * cos(previousXRotation / 57.4) - (starty * cos(previousZRotation / 57.4) - startz * sin(previousZRotation / 57.4)) * sin(previousXRotation / 57.4)) * (cos(previousYRotation / 57.4))) + (starty * sin(previousZRotation / 57.4) + startz * cos(previousZRotation / 57.4)) * sin(previousYRotation / 57.4);
					 x2 = ((endx * cos(previousXRotation / 57.4) - (endy * cos(previousZRotation / 57.4) - endz * sin(previousZRotation / 57.4)) * sin(previousXRotation / 57.4)) * (cos(previousYRotation / 57.4))) + (endy * sin(previousZRotation / 57.4) + endz * cos(previousZRotation / 57.4)) * sin(previousYRotation / 57.4);

					 y1 = ((-(startx * cos(previousXRotation / 57.4) - (starty * cos(previousZRotation / 57.4) - startz * sin(previousZRotation / 57.4)) * sin(previousXRotation / 57.4))) * (sin(previousYRotation / 57.4)) + (starty * sin(previousZRotation / 57.4) + startz * cos(previousZRotation / 57.4)) * cos(previousYRotation / 57.4));
					 y2 = ((-(endx * cos(previousXRotation / 57.4) - (endy * cos(previousZRotation / 57.4) - endz * sin(previousZRotation / 57.4)) * sin(previousXRotation / 57.4))) * (sin(previousYRotation / 57.4)) + (endy * sin(previousZRotation / 57.4) + endz * cos(previousZRotation / 57.4)) * cos(previousYRotation / 57.4));


					drawLine((x1 * prevousZoomScreenBR), (y1 * prevousZoomScreenBR), (x2 * prevousZoomScreenBR), (y2 * prevousZoomScreenBR), midX, midY,1 , 1, color);

				}

				 x1 = ((startx * cos(localRotationX / 57.4) - (starty * cos(localRotationZ / 57.4) - startz * sin(localRotationZ / 57.4)) * sin(localRotationX / 57.4)) * (cos(localRotationY / 57.4))) + (starty * sin(localRotationZ / 57.4) + startz * cos(localRotationZ / 57.4)) * sin(localRotationY / 57.4);
				 x2 = ((endx * cos(localRotationX / 57.4) - (endy * cos(localRotationZ / 57.4) - endz * sin(localRotationZ / 57.4)) * sin(localRotationX / 57.4)) * (cos(localRotationY / 57.4))) + (endy * sin(localRotationZ / 57.4) + endz * cos(localRotationZ / 57.4)) * sin(localRotationY / 57.4);

				 y1 = ((-(startx * cos(localRotationX / 57.4) - (starty * cos(localRotationZ / 57.4) - startz * sin(localRotationZ / 57.4)) * sin(localRotationX / 57.4))) * (sin(localRotationY / 57.4)) + (starty * sin(localRotationZ / 57.4) + startz * cos(localRotationZ / 57.4)) * cos(localRotationY / 57.4));
				 y2 = ((-(endx * cos(localRotationX / 57.4) - (endy * cos(localRotationZ / 57.4) - endz * sin(localRotationZ / 57.4)) * sin(localRotationX / 57.4))) * (sin(localRotationY / 57.4)) + (endy * sin(localRotationZ / 57.4) + endz * cos(localRotationZ / 57.4)) * cos(localRotationY / 57.4));



				drawLine((x1 * zoomscreen), (y1 * zoomscreen), (x2 * zoomscreen), (y2 * zoomscreen), midX, midY, 1 , 0, color);



			}

			if (zoomscreen != prevousZoomScreenBR)
			{
				prevousZoomScreenBR = zoomscreen;
			}

			previousXRotation = localRotationX;
			previousYRotation = localRotationY;
			previousZRotation = localRotationZ;


		}

		ALT_SEM_POST(sem_objectDrawn);
	}


}


/* The main function creates two task and starts multi-tasking */
int main(void)
{
	amountOfLines = 0;
	int rotatevalue[3] = {0,0,0};
	rotation = malloc(sizeof(int)*3);
	rotation = rotatevalue;

	float zoom = 0.8;
	zoomscreenpointer = malloc(sizeof(float));
	zoomscreenpointer = &zoom;

	printf("\n array set \n");

	resetScreen();
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


	OSTaskCreateExt(task2,
                  NULL,
                  (void *)&task2_stk[TASK_STACKSIZE-1],
                  TASK2_PRIORITY,
                  TASK2_PRIORITY,
                  task2_stk,
                  TASK_STACKSIZE,
                  NULL,
                  0);
  OSStart();
  return 0;
}
