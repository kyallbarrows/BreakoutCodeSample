using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawBuffer
{
    // NOTE: Graphics.CopyTexture is apparently quite fast, and would be an ideal
    // way to draw the graphics here, since it heavily leverages the GPU.
	// However, it may (or may not) have issues on iOS, which could be a
    // showstopper.  Without a way to do proper device testing, I'm falling back
    // to Get/SetPixels32 (which is also pretty speedy, from what I've read) and
    // will allow us to work with arrays, which _should_ (fingers crossed) be
    // pretty quick on most systems.  Although we can get away with nearly
    // anything at 128x128 pixels, any approach to working with bigger textures
    // would need proper benchmarking and testing on at least iOS, Android,
    // and WebGL.  Since the parts of the code that actually do the drawing are
    // pretty well contained, we could even do device specific drawing code
	// here if we really needed to using device specific code.

    // Doing the array method also incurs an overhead, and in face might even be
    // slower at this scale.  However, it would probably scale better than for
    // loops and setpixel calls.

    // TLDR: I'm going way overboard here because this is a code sample.

    private Texture2D _destinationTexture;
    private Color32[] _destinationTextureReset;

    // on every draw cycle, we'll clear this with _destinationTextureReset,
    // then draw all the elements, then copy it into _destinationTexture
    private Color32[] _drawBuffer;

    // Color32 arrays for all the sprites we'll be using
    private Dictionary<string, Color32[]> _spritePixels = new Dictionary<string, Color32[]>();

    private int _destinationWidth;
    private int _destinationHeight;

    // how far down and left we need to start drawing, since the area the game
    // runs in is smaller than the screen
    private int _topMargin;
    private int _leftMargin;

    /// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="destinationTexture">The texture to draw to.  Needs to be made readable/writable in import settings.</param>
    public DrawBuffer(Texture2D destinationTexture) {
        _destinationTexture = destinationTexture;
        _destinationWidth = destinationTexture.width;
        _destinationHeight = destinationTexture.height;
        _drawBuffer = new Color32[_destinationWidth * _destinationHeight];

        _leftMargin = (_destinationWidth - Consts.GAME_WIDTH) / 2;
        _topMargin = (_destinationHeight - Consts.GAME_HEIGHT) / 2;

        // Load up all the textures from Resources folder.
        // While it would be really great to use a sprite atlas here (and it's doable)
        // it's a bit complex, since they're set up for SpriteRenders. It can wait.
        try
        {
            // get all the texture pixels so we can do some quick array copying with them
            _spritePixels[Consts.SPRITE_HOUSE] = Resources.Load<Texture2D>(Consts.SPRITE_HOUSE).GetPixels32();
            _spritePixels[Consts.SPRITE_BALL] = Resources.Load<Texture2D>(Consts.SPRITE_BALL).GetPixels32();
            _spritePixels[Consts.SPRITE_PADDLE] = Resources.Load<Texture2D>(Consts.SPRITE_PADDLE).GetPixels32();

            // get the bricks for each row. Sprites are named by color (because that's how a real design team would
			// hand them to the dev team), but we need to store them by row index. We could just rename the files,
            // but let's pretend that the design team is constantly handing us updated files and renaming causes a
            // workflow issue.
            string[] bricksInRowColorOrder = { Consts.SPRITE_BRICK_BLUE, Consts.SPRITE_BRICK_GREEN,
                Consts.SPRITE_BRICK_YELLOW, Consts.SPRITE_BRICK_ORANGE, Consts.SPRITE_BRICK_BROWN, Consts.SPRITE_BRICK_RED };

            for (int i = 0; i < bricksInRowColorOrder.Length; i++)
            {
                string thisDigit = String.Format(Consts.SPRITE_DIGIT_TEMPLATE, i);
                _spritePixels[String.Format(Consts.SPRITE_BRICK_TEMPLATE, i)] =
                    Resources.Load<Texture2D>(bricksInRowColorOrder[i]).GetPixels32();
            }

            // store each digit
            for (int i = 0; i <= 9; i++) {
                string thisDigit = String.Format(Consts.SPRITE_DIGIT_TEMPLATE, i);
                _spritePixels[thisDigit] = Resources.Load<Texture2D>(thisDigit).GetPixels32();
            }
        }
        catch (Exception e) {
            // probably caused by a misnamed or missing resource
            Debug.LogError($"Error getting sprite pixels! {e.Message}");
        }

        try
        {
            // fill the reset texture with black
            // NOTE: the texture needs to be made writeable in the import settings.
            // While it's a teeny bit wasteful to have the color bars texture that we'll
            // just be throwing away, it's probably how we'd get it from the art team,
            // and it makes it easier to tweak the scene in editor.
            for (int x = 0; x < _destinationTexture.width; x++)
            {
                for (int y = 0; y < _destinationTexture.height; y++)
                {
                    _destinationTexture.SetPixel(x, y, Color.black);
                }
            }

            // initialize reset texture with all black pixels. 
            _destinationTextureReset = _destinationTexture.GetPixels32();
            Blit(_spritePixels[Consts.SPRITE_HOUSE], Consts.HOUSE_WIDTH, Consts.HOUSE_HEIGHT,
                _destinationTextureReset, _leftMargin, _topMargin, _destinationWidth);
        }
        catch (Exception e)
        {
            // Texture probably isn't writable, may need to fix in import settings
            Debug.LogError($"Error clearing out main screen texture: {e.Message}");
        }

        // now that we've got both textures filled with black, we can speed renders up by
        // pre-drawing the house into the buffer reset image, since it never changes.
        // Not sure what the "house" is really called, but I'm calling the
        // gray upside-down U shape that constrains the ball the "house", because
        // it's where the ball lives.

    }

    /// <summary>
	/// Resets the buffer to initial state so we can draw a new frame
	/// </summary>
    public void BeginDrawCycle() {
        Blit(_destinationTextureReset, _destinationWidth, _destinationHeight,
            _drawBuffer, 0, 0, _destinationWidth);
    }

    /// <summary>
	/// Copies parts of one texture into another texture.
	/// </summary>
	/// <param name="src">The Color32 array to copy from</param>
	/// <param name="sourceWidth">Width of the source sprite</param>
	/// <param name="sourceHeight">Height of the source sprite</param>
	/// <param name="dest">The Color32 array to write to</param>
	/// <param name="x">The x position to write to</param>
	/// <param name="y">The x position to write to</param>
	/// <param name="destWidth">The width of the destination texture</param>
	private void Blit(Color32[] src, int srcWidth, int srcHeight, Color32[] dest, int x, int y, int destWidth) {
        // since the 2-dimensional texture is now a 1-dimensional array, we can't just block copy.
        // we'll need to copy one row at a time, which should still be faster than nested for-loops and SetPixel() calls.
        for (int i = 0; i < srcHeight; i++) {
            Array.Copy(src, i * srcWidth, dest, (i + y) * destWidth + x, srcWidth);
        }
    }

    /// <summary>
	/// Convenience method for Blit'ing into _drawBuffer.
	/// </summary>
	/// <param name="src">The Color32 array to copy from</param>
	/// <param name="sourceWidth">Width of the source sprite</param>
	/// <param name="sourceHeight">Height of the source sprite</param>
	/// <param name="x">The x position to write to</param>
	/// <param name="y">The x position to write to</param>
    private void DrawElement(Color32[] src, int sourceWidth, int sourceHeight, int x, int y)
    {
        Blit(src, sourceWidth, sourceHeight, _drawBuffer, _leftMargin + x, _topMargin + y, _destinationWidth);
    }

    /// <summary>
	/// Draws a number digit at top of screen
	/// </summary>
	/// <param name="digit">The value, 0-9, to draw</param>
	/// <param name="x">x of left side of digit</param>
	/// <param name="y">y of bottom of digit</param>
    public void DrawNumber(int digit, int x, int y) {
        // get the pixels for this digit
        Color32[] pixels;
        try {
            pixels = _spritePixels[String.Format(Consts.SPRITE_DIGIT_TEMPLATE, digit)];
        }
        catch (Exception e) {
            // number out of 0-9 range?
            Debug.LogError($"Couldn't load digit pixels for {digit}: {e.Message}");
            return;
        }

        // copy the pixels into draw buffer
        DrawElement(pixels, Consts.DIGIT_WIDTH, Consts.DIGIT_HEIGHT, x, y);
    }

    /// <summary>
	/// Draws a brick
	/// </summary>
	/// <param name="color">A Palette enum value for coloring the brick</param>
	/// <param name="x">x of the left side of the brick</param>
	/// <param name="y">y of the bottom of the brick</param>
    public void DrawBrick(int columnIndex, int rowIndex) {
        var pixels = _spritePixels[string.Format(Consts.SPRITE_BRICK_TEMPLATE, rowIndex)];
        DrawElement(pixels, Consts.BRICK_WIDTH, Consts.BRICK_HEIGHT,
            columnIndex * Consts.BRICK_WIDTH + Consts.HOUSE_WALL_THICKNESS, Consts.BRICKS_START_Y + rowIndex * Consts.BRICK_HEIGHT);
    }

    /// <summary>
	/// Draws the paddle
	/// </summary>
	/// <param name="x">x of the left side of the paddle</param>
	/// <param name="y">y of the paddle</param>
    public void DrawPaddle(int x, int y) {
        DrawElement(_spritePixels[Consts.SPRITE_PADDLE], Consts.PADDLE_WIDTH, Consts.PADDLE_HEIGHT, x + Consts.HOUSE_WALL_THICKNESS, y);
    }

    /// <summary>
	/// Draws the ball
	/// </summary>
	/// <param name="x">x</param>
	/// <param name="y">y</param>
    public void DrawBall(int x, int y) {
        DrawElement(_spritePixels[Consts.SPRITE_BALL], Consts.BALL_WIDTH, Consts.BALL_HEIGHT, x + Consts.HOUSE_WALL_THICKNESS, y);
    }

    /// <summary>
	/// Takes all the things you've drawn and puts them into the destination texture.
	/// </summary>
    public void FinishDrawCycle() {
        _destinationTexture.SetPixels32(_drawBuffer);
        _destinationTexture.Apply(false);
    }
}
