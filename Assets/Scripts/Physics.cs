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

	private float _ballAngle;
	private Vector2 _ballVelocity;
	private Vector2Int _lastPixelBallPosition;
	private Vector2 _preciseBallPosition;

	// gets the closest pixel position of the precise ball location
	public Vector2Int PixelBallPosition => new Vector2Int(
		(int)Mathf.Round(_preciseBallPosition.x),
		(int)Mathf.Round(_preciseBallPosition.y));

	private int _paddleLeftX;
	public int PaddleLeftX => _paddleLeftX;


	public Physics() { }

	// TODO:
	// have a run-frame function
	// track ball pos, velocity
	// track bricks, make accessible
	// make ball and paddle accessible
	// have way to set paddle pos
	// have a way to set ball speed, maybe in constructor?
	// raise events on wall/paddle bumps or brick hits?

	public void MovePaddleTo(float leftRatio) {
		_paddleLeftX = Consts.HOUSE_WALL_THICKNESS + (int)(leftRatio * (float)Consts.PADDLE_MOVE_RANGE);
	}

	public void Reset(float ballSpeed) {
		// value type, so we're ok chaining the assignments
		_preciseBallPosition = new Vector2(Consts.INITIAL_BALL_X, Consts.INITIAL_BALL_Y);
		_lastPixelBallPosition = PixelBallPosition;

		_ballAngle = Consts.BALL_ANGLE_MEDIUM;
		_ballVelocity = new Vector2(ballSpeed * Mathf.Cos(_ballAngle), ballSpeed * Mathf.Sin(_ballAngle));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="deltaT">Elapsed seconds since last call, used for moving the ball and paddle</param>
	/// <returns>Whether the ball is still alive</returns>
	public bool RunStep(float deltaT) {
		var lastCheckedBallPos = _lastPixelBallPosition;

		// We need to check every pixel the ball travels through, especially the
		// faster it goes.  Otherwise, it may warp past a brick or a wall, which
		// seems to happen fairly often in the original.
		// If we only travel one angled-pixel at a time, we will ensure that we hit
		// every x/y pixel in between current and destination positions.
		// NOTE: we could optimize out the expensive square root call on .magnitude
		// here by modifying the end condition on the for loop.  It's only once
		// per frame, so no need, but if it was 10,000 calls, we'd probably fix it.
		// Furthermore, _ballVelocity only changes on paddle hits.  It reads better like this,
		// but should probably be moved to the paddle hit code, and the value stored.
		float ballDistance = deltaT * _ballVelocity.magnitude;

		// There are 3 scenarios here:
		// 1) we may not even have traveled a full pixel.  In that case, we should
		//    bail out
		// 2) We may have traveled one pixel.  Check the new position for hits
		// 3) We may have traveled more than one pixel.  Check all in between pixels

		// move ball no more than one pixel at a time.  Unless the game is running
		// really slow, or ball is really fast, distance will probably be a very
		// small float value, and we'll only check physics every few frames.
		// moving only one pixel at a time ensures we hit every pixel in the travel
		// path, so as not to warp through any bricks
		var preciseStartingBallPos = _preciseBallPosition;

		int protect = 0;

		for (float traveled = 0; traveled <= ballDistance && protect < 10000; ) {
			protect++;
			var thisTravel = Mathf.Min(1f, ballDistance);
			traveled += thisTravel;
			// NOTE: _ballAngle might get modified between loops here by DoPhysics, 
			// but speed will stay constant
			Vector2 direction = _ballVelocity.normalized;
			Vector2 preciseTotalTravel = new Vector2(direction.x * thisTravel, direction.y * thisTravel);
			_preciseBallPosition = preciseStartingBallPos + preciseTotalTravel;
			// make sure it actually moved a full pixel before checking physics here
			if (PixelBallPosition != _lastPixelBallPosition) {
				_lastPixelBallPosition = PixelBallPosition;
				if (!DoHitChecks(_lastPixelBallPosition)) {
					return false;
				}
			}
		}

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

	private bool DoHitChecks(Vector2Int ballPos) {
		// check if below paddle
		if (ballPos.y <= Consts.PADDLE_Y) {
			// lost a life
			return false;
		}

		bool hitTop = false;
		bool hitSide = false;
		bool hitPaddle = false;

		// check top hit, reverse direction if so
		// only reverse it if we're headed up though, we may have a very shallow
		// angle and be running close enough to top that it looks like a hit
		if (ballPos.y >= Consts.TOP_WALL_Y && _ballVelocity.y > 0) {
			_ballVelocity.y *= -1;
			hitTop = true;
		}

		if (ballPos.x <= Consts.LEFT_WALL_X && _ballVelocity.x < 0) {
			_ballVelocity.x *= -1;
			hitSide = true;
		}
		else if (ballPos.x >= Consts.RIGHT_WALL_X && _ballVelocity.x > 0)
		{
			_ballVelocity.x *= -1;
			hitSide = true;
		}

		// check for paddle hit
		if (ballPos.y <= Consts.PADDLE_Y + 1 && _ballVelocity.y < 0 &&
			ballPos.x >= _paddleLeftX && ballPos.x <= (_paddleLeftX + Consts.PADDLE_WIDTH)) {
			_ballVelocity.y *= -1;
			hitPaddle = true;
		}

		// TODO: if we ever add achievements, we should really award one called
		// "DVD Screensaver" if they hit top and side at same time
		if (hitTop || hitSide || hitPaddle)
		{
			// TODO: play a sound
		}

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
