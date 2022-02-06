using System;
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
	private BitArray[] _bricks = new BitArray[Consts.NUM_BRICK_ROWS];

	private float _ballAngle;
	private float _ballSpeed;
	private Vector2 _ballVelocity;
	private Vector2Int _lastPixelBallPosition;
	private Vector2 _preciseBallPosition;



	// gets the closest pixel position of the precise ball location
	public Vector2Int PixelBallPosition => new Vector2Int(
		(int)Mathf.Round(_preciseBallPosition.x),
		(int)Mathf.Round(_preciseBallPosition.y));

	private int _paddleLeftX;
	public int PaddleLeftX => _paddleLeftX;


	public Physics() {
	}

	// TODO:
	// have a run-frame function
	// track ball pos, velocity
	// track bricks, make accessible
	// make ball and paddle accessible
	// have way to set paddle pos
	// have a way to set ball speed, maybe in constructor?
	// raise events on wall/paddle bumps or brick hits?

	public void MovePaddleTo(float leftRatio) {
		_paddleLeftX = (int)(leftRatio * (float)Consts.PADDLE_MOVE_RANGE);
	}

	public void Reset(float ballSpeed) {
		for (int row = 0; row < Consts.NUM_BRICK_ROWS; row++) {
			// create new BitVector for each row of bricks, setting all bits to true
			_bricks[row] = new BitArray(Consts.BRICKS_PER_ROW, true);
		}

		_ballSpeed = ballSpeed;

		// value type, so we're ok chaining the assignments
		_preciseBallPosition = new Vector2(Consts.INITIAL_BALL_X, Consts.INITIAL_BALL_Y);
		_lastPixelBallPosition = PixelBallPosition;

		_ballAngle = Consts.BALL_ANGLE_MEDIUM;
		SetBallVelocity(_ballAngle, _ballSpeed);
	}

	private void SetBallVelocity(float angle, float speed) {
		_ballVelocity = new Vector2(speed * Mathf.Cos(angle), speed * Mathf.Sin(angle));
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

		if (ballPos.x <= Consts.BALL_LEFT_LIMIT && _ballVelocity.x < 0) {
			_ballVelocity.x *= -1;
			hitSide = true;
		}
		else if (ballPos.x >= Consts.BALL_RIGHT_LIMIT && _ballVelocity.x > 0)
		{
			_ballVelocity.x *= -1;
			hitSide = true;
		}

		for (int row = 0; row < Consts.NUM_BRICK_ROWS; row++) {
			var thisRow = _bricks[row];
			var rowY = Consts.BRICKS_START_Y + row * Consts.BRICK_HEIGHT;
			if (ballPos.y >= rowY && ballPos.y < rowY + Consts.BRICK_HEIGHT) {
				// check this row
				int colIndex = ballPos.x / Consts.BRICK_WIDTH;

				// if we have a brick, reflect 
				if (thisRow[colIndex]) {
					// this is a little inaccurate, as we'll count a hit from below on the end
					// as a side hit.  We could improve by trackig last position, and seeing which angle
					// it came in from.  For now though, this will suffice
					int leftEdge = colIndex * Consts.BRICK_WIDTH;
					int rightEdge = leftEdge + Consts.BRICK_WIDTH - 1;

					if (ballPos.x >= leftEdge && ballPos.x <= rightEdge) {
						// disable the brick
						thisRow[colIndex] = false;

						// if it was hit on the left, from the left, or on the
						// right, from the right, rebound x
						if ((ballPos.x == leftEdge && _ballVelocity.x > 0) ||
							(ballPos.x == rightEdge && _ballVelocity.x < 0))
						{
							_ballVelocity.x *= -1;
						}
						else {
							// assume hit from below, rebound y
							_ballVelocity.y *= -1;
						}
					}
				}
			}
		}

		// check for paddle hit
		if (ballPos.y <= Consts.PADDLE_Y + 1 && _ballVelocity.y < 0 &&
			ballPos.x >= _paddleLeftX && ballPos.x <= (_paddleLeftX + Consts.PADDLE_WIDTH)) {
			// need to check position of ball on paddle.
			// If it's on front half, just reverse Y velocity
			// If it's on the back half, may need to change bounce angle.
			int distanceFromPaddleTail = ballPos.x - _paddleLeftX;
			if (_ballVelocity.x < 0) {
				distanceFromPaddleTail = _paddleLeftX + Consts.PADDLE_WIDTH - ballPos.x;
			}

			int xDirection = _ballVelocity.x > 0 ? 1 : -1;
			if (distanceFromPaddleTail <= 1)
			{
				_ballAngle = Consts.BALL_ANGLE_SHALLOW;
				xDirection *= -1;
			}
			else if (distanceFromPaddleTail <= 3)
			{
				_ballAngle = Consts.BALL_ANGLE_MEDIUM;
				xDirection *= -1;
			}
			else if (distanceFromPaddleTail < 5)
			{
				_ballAngle = Consts.BALL_ANGLE_DEEP;
				xDirection *= -1;
			}
			else {
				// no chnage needed, we'll reverse y direction for all these cases at the end
			}

			SetBallVelocity(_ballAngle, _ballSpeed);
			_ballVelocity.x = Mathf.Abs(_ballVelocity.x) * xDirection;
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
	public BitArray GetBrickRow(int rowIndex) {
		// TODO: add some guardrails on the index here
		return _bricks[rowIndex];
	}

}
