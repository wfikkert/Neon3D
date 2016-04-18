/* globals */
#define BUF_SIZE 500000			// about 10 seconds of buffer (@ 48K samples/sec)
#define BUF_THRESHOLD 96		// 75% of 128 word buffer

#include <stdio.h>
#include "altera_up_avalon_character_lcd.h"
#include "altera_up_avalon_parallel_port.h"
#include "string.h"
#include <os/alt_sem.h>
#include <math.h>

/* function prototypes */
void LCD_cursor( int, int );
void LCD_text( char * );
void LCD_cursor_off( void );
void VGA_text (int, int, char *);
void VGA_box (int, int, int, int, short);
void HEX_PS2(char, char, char);
void check_KEYs( int *, int *, int * );


void drawPixel(int x, int y, int size, short pixel_color )
{
	VGA_box (x, y, x + size - 1, y + size - 1, pixel_color);
}

void removePixel(int x, int y, int size)
{
	VGA_box (x, y, x +(size - 1), y + (size - 1), 0x0000);
}


void drawLine(double x1, double y1, double x2, double y2, double midX, double midY, int size, int remove, short pixel_color)
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
    double x = 0;
    double y = 0;
    double prevY = 0;
    double prevX = 0;
    //for loop for drawing beam
    for (x = x1; x <= x2; x++)
    {
    		y = 0;
            //formula for drawing beam between nodes
			if ((x2 - x1) == 0)
			{
                double count;

                if (y2 > y1)
                {
                    double tempY = y1;
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
            	double a = y2 - y1;
            	double b = x2 - x1;
            	double c = a /b;
            	double d = x - x1;
            	double e = c * d;
                y = e + y1;
            }

			 //drawing all pixels of beam between nodes
			        if (y - prevY > 0 && x != 0)
			        {

			            double counter;
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
			            double counter;
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

			        //draw line/remove prev line
			        /*
			        if (!remove)
			        {
			            drawPixel((int)x, (int)y, size, pixel_color);
			        }
			        else
			        {
			            removePixel((int)x, (int)y, size);
			        }*/

			        prevY = y;
			        prevX = x;

        }

}

double prevousZoomScreenBR = 0;
int previousXRotation = 0;
int previousYRotation = 0;

void drawNodesAndBeams( double zoomscreen, int rotation[2], double starteEndnodes[8][2][3], int midX, int midY, short color)
{
	int linesDrawn;
	double startx = 0;
	double starty = 0;
	double startz = 0;
	double endx = 0;
	double endy = 0;
	double endz = 0;

	for (linesDrawn = 0; linesDrawn < 8; linesDrawn++)
	{


		double x1 = startx = starteEndnodes[linesDrawn][0][0];
		double y1 = starty = starteEndnodes[linesDrawn][0][1];
		double z1 = startz = starteEndnodes[linesDrawn][0][2];

		double x2 = endx = starteEndnodes[linesDrawn][1][0];
		double y2 = endy = starteEndnodes[linesDrawn][1][1];
		double z2 = endz = starteEndnodes[linesDrawn][1][2];


		if(previousXRotation != rotation[0] || previousYRotation != rotation[1])
		{
			x1 = ((startx * cos(previousXRotation / 57.4) - starty * sin(previousXRotation / 57.4)) * (cos(previousYRotation / 57.4))) + startz * sin(previousYRotation / 57.4);
			x2 = ((endx * cos(previousXRotation / 57.4) - endy * sin(previousXRotation / 57.4)) * (cos(previousYRotation / 57.4))) + endz * sin(previousYRotation / 57.4);

			z1 = ((-(startx * cos(previousXRotation / 57.4) - starty * sin(previousXRotation / 57.4))) * (sin(previousYRotation / 57.4)) + startz * cos(previousYRotation / 57.4));
			z2 = ((-(endx * cos(previousXRotation / 57.4) - endy * sin(previousXRotation / 57.4))) * (sin(previousYRotation / 57.4)) + endz * cos(previousYRotation / 57.4));

			y1 = z1;
			y2 = z2;

			drawLine((x1 * prevousZoomScreenBR), (y1 * prevousZoomScreenBR), (x2 * prevousZoomScreenBR), (y2 * prevousZoomScreenBR), midX, midY, 1 , 1, color);

		}

		x1 = ((startx * cos(rotation[0] / 57.4) - starty * sin(rotation[0] / 57.4)) * (cos(rotation[1] / 57.4))) + startz * sin(rotation[1] / 57.4) ;
		x2 = ((endx * cos(rotation[0] / 57.4) - endy * sin(rotation[0] / 57.4))*(cos(rotation[1] / 57.4))) + endz * sin(rotation[1] / 57.4);

		z1 = ((-(startx * cos(rotation[0] / 57.4) - starty * sin(rotation[0] / 57.4))) *(sin(rotation[1] / 57.4)) + startz * cos(rotation[1] / 57.4));
		z2 = ((-(endx * cos(rotation[0] / 57.4) - endy * sin(rotation[0] / 57.4))) * (sin(rotation[1] / 57.4)) + endz * cos(rotation[1] / 57.4));

		y1 = z1;
		y2 = z2;

		if (zoomscreen != prevousZoomScreenBR)
		{
			drawLine((x1 * prevousZoomScreenBR), (y1 * prevousZoomScreenBR), (x2 * prevousZoomScreenBR), (y2 * prevousZoomScreenBR), midX, midY, 1 , 1, color);

			drawLine((x1 * zoomscreen), (y1 * zoomscreen), (x2 * zoomscreen), (y2 * zoomscreen),midX, midY, 1 , 0, color);

		}
		else
		{
			drawLine((x1 * zoomscreen), (y1 * zoomscreen), (x2 * zoomscreen), (y2 * zoomscreen), midX, midY, 1 , 0, color);

		}

	}


	if (zoomscreen != prevousZoomScreenBR)
	{
		prevousZoomScreenBR = zoomscreen;
	}
	previousXRotation = rotation[0];
	previousYRotation = rotation[1];
}


/********************************************************************************
 * This program demonstrates use of the media ports in the DE2 Media Computer
 *
 * It performs the following:
 *  	1. records audio for about 10 seconds when KEY[1] is pressed. LEDG[0] is
 *  	   lit while recording
 * 	2. plays the recorded audio when KEY[2] is pressed. LEDG[1] is lit while
 * 	   playing
 * 	3. Draws a blue box on the VGA display, and places a text string inside
 * 	   the box
 * 	4. Shows a text message on the LCD display
 * 	5. Displays the last three bytes of data received from the PS/2 port
 * 	   on the HEX displays on the DE2 board
********************************************************************************/
int main(void)
{
	/* Declare volatile pointers to I/O registers (volatile means that IO load
	   and store instructions will be used to access these pointer locations,
	   instead of regular memory loads and stores) */
  	volatile int * green_LED_ptr = (int *) 0x10000010;		// green LED address
	volatile int * audio_ptr = (int *) 0x10003040;			// audio port address
	volatile int * PS2_ptr = (int *) 0x10000100;				// PS/2 port address

	/* used for audio record/playback */
	int fifospace, leftdata, rightdata;
	int record = 0, play = 0, buffer_index = 0;
	int left_buffer[BUF_SIZE];
	int right_buffer[BUF_SIZE];

	/* used for PS/2 port data */
	int PS2_data, RVALID;
	char byte1 = 0, byte2 = 0, byte3 = 0;

	/* create a message to be displayed on the VGA and LCD displays */




	double startEndNodes[8][2][3] = {	{{0,100,0},{100,0,100}},
										{{0,100,0},{100,0,-100}},
										{{0,100,0},{-100,0,100}},
										{{0,100,0},{-100,0,-100}},
										{{-100,0,-100},{-100,0,100}},
										{{100,0,-100},{100,0,100}},
										{{-100,0,-100},{100,0,-100}},
										{{-100,0,100},{100,0,100}},
										};

	/* output text message to the LCD */
	//LCD_cursor (0,0);										// set LCD cursor location to top row
	//LCD_text (text_top_row);
	//LCD_cursor (0,1);										// set LCD cursor location to bottom row
	//LCD_text (text_bottom_row);
	//LCD_cursor_off ();									// turn off the LCD cursor

	/* output text message in the middle of the VGA monitor */
	VGA_box (0, 0, 319, 239, 0);						// clear the screen
	//VGA_box (0, 0, 320, 240, 0x00FF);

	//VGA_text (1, 1, text_top_row);
	/* read and echo audio data */
	record = 0;
	play = 0;

	// PS/2 mouse needs to be reset (must be already plugged in)
	*(PS2_ptr) = 0xFF;		// reset


	//volatile int * JTAG_UART_ptr 	= (int *) JTAG_UART_BASE;



	int rotation[2] = {90,0};

	int xRotate = 0;
	int yRotate = 50;
	double zoom = 50;
	while(1)
	{
			if(xRotate > 359){
				xRotate = 0;
			}

			if(yRotate > 359){
				yRotate = 0;
			}

			if(zoom == 80){
				zoom = 50;
			}
			rotation[0] = xRotate;
			rotation[1] = yRotate;
			drawNodesAndBeams(zoom / 100,rotation, startEndNodes, 160, 120, 0x0F00);
			drawPixel(159, 119, 2, 0xF000);
			xRotate++;
			yRotate++;
			zoom++;
			char text_top_row[40];
			char text_bottom_row[40];
			char text_bottom_row2[40];
			char text_bottom_row3[40] = "SOFTWARE MODE \0";
			sprintf(text_top_row, "3D amount of lines: %d", 8);
			VGA_text (1, 1, "                                          \0");
			VGA_text (1, 2, "                                          \0");
			VGA_text (1, 3, "                                          \0");
			VGA_text (1, 1, text_top_row);

			sprintf(text_bottom_row, "Zoom: %f", zoom / 100);
			VGA_text (1, 2, text_bottom_row);

			sprintf(text_bottom_row2, "Rotation X,Y: %d,%d", rotation[0], rotation[1]);
			VGA_text (1, 3, text_bottom_row2);

			VGA_text (1, 4, text_bottom_row3);
		/*
		data = *(JTAG_UART_ptr);		 		// read the JTAG_UART data register
		if (data & 0x00008000)					// check RVALID to see if there is new data
		{
			data = data & 0x000000FF;			// the data is in the least significant byte
			printf("%c", (char)data);

			char customString[2] = {(char)data, '\0'};
			strcpy(text_bottom_row, customString);
			if(data == 'd'){
				h++;
			}
		}
		 */
	}
}

/****************************************************************************************
 * Subroutine to move the LCD cursor
****************************************************************************************/
void LCD_cursor(int x, int y)
{
  	volatile char * LCD_display_ptr = (char *) 0x10003050;	// 16x2 character display
	char instruction;

	instruction = x;
	if (y != 0) instruction |= 0x40;				// set bit 6 for bottom row
	instruction |= 0x80;								// need to set bit 7 to set the cursor location
	*(LCD_display_ptr) = instruction;			// write to the LCD instruction register
}

/****************************************************************************************
 * Subroutine to send a string of text to the LCD
****************************************************************************************/
void LCD_text(char * text_ptr)
{
  	volatile char * LCD_display_ptr = (char *) 0x10003050;	// 16x2 character display

	while ( *(text_ptr) )
	{
		*(LCD_display_ptr + 1) = *(text_ptr);	// write to the LCD data register
		++text_ptr;
	}
}

/****************************************************************************************
 * Subroutine to turn off the LCD cursor
****************************************************************************************/
void LCD_cursor_off(void)
{
  	volatile char * LCD_display_ptr = (char *) 0x10003050;	// 16x2 character display
	*(LCD_display_ptr) = 0x0C;											// turn off the LCD cursor
}

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

/****************************************************************************************
 * Subroutine to show a string of HEX data on the HEX displays
****************************************************************************************/
void HEX_PS2(char b1, char b2, char b3)
{
	volatile int * HEX3_HEX0_ptr = (int *) 0x10000020;
	volatile int * HEX7_HEX4_ptr = (int *) 0x10000030;

	/* SEVEN_SEGMENT_DECODE_TABLE gives the on/off settings for all segments in
	 * a single 7-seg display in the DE2 Media Computer, for the hex digits 0 - F */
	unsigned char	seven_seg_decode_table[] = {	0x3F, 0x06, 0x5B, 0x4F, 0x66, 0x6D, 0x7D, 0x07,
		  										0x7F, 0x67, 0x77, 0x7C, 0x39, 0x5E, 0x79, 0x71 };
	unsigned char	hex_segs[] = { 0, 0, 0, 0, 0, 0, 0, 0 };
	unsigned int shift_buffer, nibble;
	unsigned char code;
	int i;

	shift_buffer = (b1 << 16) | (b2 << 8) | b3;
	for ( i = 0; i < 6; ++i )
	{
		nibble = shift_buffer & 0x0000000F;		// character is in rightmost nibble
		code = seven_seg_decode_table[nibble];
		hex_segs[i] = code;
		shift_buffer = shift_buffer >> 4;
	}
	/* drive the hex displays */
	*(HEX3_HEX0_ptr) = *(int *) (hex_segs);
	*(HEX7_HEX4_ptr) = *(int *) (hex_segs+4);
}

/****************************************************************************************
 * Subroutine to read KEYs
****************************************************************************************/
void check_KEYs(int * KEY1, int * KEY2, int * counter)
{
	volatile int * KEY_ptr = (int *) 0x10000050;		// pushbutton KEY address
	volatile int * audio_ptr = (int *) 0x10003040;	// audio port address
	int KEY_value;

	KEY_value = *(KEY_ptr); 				// read the pushbutton KEY values
	while (*KEY_ptr);							// wait for pushbutton KEY release

	if (KEY_value == 0x2)					// check KEY1
	{
		// reset counter to start recording
		*counter = 0;
		// clear audio-in FIFO
		*(audio_ptr) = 0x4;
		*(audio_ptr) = 0x0;

		*KEY1 = 1;
	}
	else if (KEY_value == 0x4)				// check KEY2
	{
		// reset counter to start playback
		*counter = 0;
		// clear audio-out FIFO
		*(audio_ptr) = 0x8;
		*(audio_ptr) = 0x0;

		*KEY2 = 1;
	}
}
