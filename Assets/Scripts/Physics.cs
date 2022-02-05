using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public struct Brick {
	int left;
	int bottom;
}

public class Physics
{
	// Track whether the bricks are alive or not using a BitVector32 for each
	// row of bricks.  
	private BitVector32[] brick = new BitVector32[Consts.NUM_BRICK_ROWS];

	private Vector2 ballVelocity;
	private Vector2 ballPosition;
	public Vector2 BallPosition => ballPosition;

	private int paddleLeftX;
	public int PaddleLeftX => paddleLeftX;


	public Physics(float ballSpeed) { }

	// TODO:
	// have a run-frame function
	// track ball pos, velocity
	// track bricks, make accessible
	// make ball and paddle accessible
	// have way to set paddle pos
	// have a way to set ball speed, maybe in constructor?
	// raise events on wall/paddle bumps or brick hits?

	public void MovePaddleTo(float leftRatio) { }

	public void MovePaddleRight() { }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="deltaT">Elapsed seconds since last call, used for moving the ball and paddle</param>
	/// <returns>Whether anything changed. If nothing did, no need to do a redraw</returns>
	public bool RunStep(float deltaT) {
		// calculate next ball position, rounding to int's
		// if it's the same, do nothing

		// if ball has moved more than one 1 pixel, we need to check the in
		// between pixels, so use Bresenham's algorithm to get each interstitial
		// pixel value

		// with each pixel we touched:
		// - do hit detection
		// - remove any dead bricks
		// - update ball velocity
		// - add to list of sounds we need to play

		return true;
	}

	/// <summary>
	/// Gets all the bricks, so they can be rendered
	/// </summary>
	/// <returns></returns>
	public object GetBricks() {
		return null;
	}

}
