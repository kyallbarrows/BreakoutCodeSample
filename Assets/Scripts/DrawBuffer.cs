using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBuffer
{
    private const int NUM_COLORS = 7;
    public enum UsableColors {
        GRAY,
        BLUE,
        GREEN,
        YELLOW,
        ORANGE,
        DEEP_ORANGE,
        RED
    }

    // our buffer will be wider than our usable width.  This is because the really old school
    // consoles never seemed to go all the way to the edges.  
    // We'll have buffer size, which is the total drawable area, and usable size, which 
    // is the portion we'll be drawing into.  Later, if we wanted to, we could leverage
    // some of the marginal area and do some really cool vsync error effects.
    private const int BUFFER_WIDTH = 128;

    // Though I've googled it a bit, I'm unclear on whether Unity really needs
	// npo2 texture sizes anymore.  However, since the spec doesn't really
	// mention where this will run, we'll play it safe and keep everything a
	// power of 2
    private const int BUFFER_HEIGHT = 128;

    // these are the actual dimensions we'll be drawing into. 
    private const int USABLE_WIDTH = 100;
    private const int USABLE_HEIGHT = 62;

    // how far down and left we need to start drawing
    private const int TOP_MARGIN = (BUFFER_HEIGHT - USABLE_HEIGHT) / 2;
    private const int LEFT_MARGIN = (BUFFER_WIDTH - USABLE_WIDTH) / 2;

    // Using RGB24 color, so 3 bytes per pixel.
    private const int PIXEL_STRIDE = 3;

    // The actual buffer we'll be creating, that we can use to set pixels on the
    // Texture2D the TV screen will be textured with
    private byte[] buffer = new byte[BUFFER_WIDTH * BUFFER_HEIGHT * PIXEL_STRIDE];

    // we'll use this to set the initial state of the buffer. It will contain the parts of 
    // the game that never change (the big frame around the bricks)
    private byte[] initialBufferState = new byte[BUFFER_WIDTH * BUFFER_HEIGHT * PIXEL_STRIDE];

    // To speed things up, we'll draw strips of same-colored pixels in one go
    // using a memory copy function.  This should be a bit faster than for-loops
    // and SetPixel() calls.  We'll need a strip for each color that we can copy
    // from. 
    private byte[,] colorsStrips = new byte[NUM_COLORS, USABLE_WIDTH];

    // width and height of the digit characters at top of screen
    private const int NUMBER_HEIGHT = 5;
    private const int NUMBER_WIDTH = 3;
    // numbers are stretched 3x, so each pixel is repeated 3 times sideways
    private const int NUMBER_STRETCH = 3;
    private const int DIGIT_SPACING = 1;

    // 3x5 pixel bitmaps for each digit that we'll draw in the score area.
    // Read left to right, bottom to top.
    // We're using a whole byte for a bit-value, so if we were really concerned about the 
    // .00003kb, we could use a BitArray. However, the overhead of importing
    // the class would likely outweight any gains.  Bitwise operators on hex values would
    // be ideal, but this is much easier to read.
    private byte[] numberPixels = new byte[] {
            1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, // 0
            0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, // 1
            1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, // 2
            1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, // 3
            0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, // 4
            1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, // 5
            1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, // 6
            0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, // 7
            1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, // 8
            0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1  // 9
        };

    public DrawBuffer() {
    }

    /// <summary>
	/// Resets the buffer to initial state so we can draw a new frame
	/// </summary>
    public void ResetBuffer() {

    }

    /// <summary>
	/// Draws a number digit at top of screen
	/// </summary>
	/// <param name="digit">The value, 0-9, to draw</param>
	/// <param name="x">x of left side of digit</param>
	/// <param name="y">y of bottom of digit</param>
    public void DrawNumber(int digit, int x, int y) {
        
    }

    /// <summary>
	/// Draws a brick
	/// </summary>
	/// <param name="color">A UsableColors enum value for coloring the brick</param>
	/// <param name="x">x of the left side of the brick</param>
	/// <param name="y">y of the bottom of the brick</param>
    public void DrawBrick(int rowIndex, int x, int y) {

    }

    /// <summary>
	/// Draws the paddle
	/// </summary>
	/// <param name="x">x of the left side of the paddle</param>
	/// <param name="y">y of the paddle</param>
    public void DrawPaddle(int x, int y) {

    }

    /// <summary>
	/// Draws the ball
	/// </summary>
	/// <param name="x">x</param>
	/// <param name="y">y</param>
    public void DrawBall(int x, int y) {

    }    
}
