using System.Collections;
using UnityEngine;

public class PhysicsStepResult {
	public int ScoreChange = 0;
	public bool LifeLost = false;
	public bool WonLevel = false;
}

/// <summary>
/// Manages hit detection and moving the ball around.
/// </summary>
public class Physics
{
	// Track whether the bricks are alive or not using a BitArray for each
	// row of bricks.  
	private BitArray[] _bricks = new BitArray[Consts.NUM_BRICK_ROWS];
	private int _numRemainingBricks;

	private bool _ballAlive = true;
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

	/// <summary>
	/// Moves the paddle to the provided location, in the range 0 to 1 (inclusive)
	/// </summary>
	/// <param name="leftRatio">How far from the left to move the paddle, in the range 0 to 1</param>
	public void MovePaddleTo(float leftRatio) {
		_paddleLeftX = (int)(leftRatio * (float)Consts.PADDLE_MOVE_RANGE);
	}

	/// <summary>
	/// Resets the physics so the next level can begin.
	/// </summary>
	/// <param name="ballSpeed">The new movement speed of the ball for this level.</param>
	public void Reset(float ballSpeed) {
		_numRemainingBricks = Consts.NUM_BRICK_ROWS * Consts.BRICKS_PER_ROW;

		for (int row = 0; row < Consts.NUM_BRICK_ROWS; row++) {
			// create new BitArray for each row of bricks, setting all bits to true
			_bricks[row] = new BitArray(Consts.BRICKS_PER_ROW, true);
		}

		_ballSpeed = ballSpeed;
		_ballAngle = Consts.BALL_ANGLE_MEDIUM;
		SetBallVelocity(_ballAngle, _ballSpeed);

		ResetBall();
	}

	/// <summary>
	/// Reset just the ball to the starting position.  Useful if the player just
	/// died and needs to start on the next life.
	/// </summary>
	public void ResetBall() {
		_preciseBallPosition = new Vector2(Consts.INITIAL_BALL_X, Consts.INITIAL_BALL_Y);
		_lastPixelBallPosition = PixelBallPosition;
		_ballAlive = true;
	}

	private void SetBallVelocity(float angle, float speed) {
		_ballVelocity = new Vector2(speed * Mathf.Cos(angle), speed * Mathf.Sin(angle));
	}

	/// <summary>
	/// Runs a single step of the physics.  This needs some improvement.
	/// </summary>
	/// <param name="deltaT">Elapsed seconds since last call, used for moving the ball and paddle</param>
	/// <returns>A PhysicsStepResult instance containing any score change, whether they died, and whether they won</returns>
	public PhysicsStepResult RunStep(float deltaT) {
		// DONT run if the ball is dead, it just racks up losses unfairly.
		if (!_ballAlive) {
			return new PhysicsStepResult();
		}

		// We need to check every pixel the ball travels through, especially the
		// faster it goes.  Otherwise, it may warp past a brick or a wall, which
		// seems to happen fairly often in the original.
		// If we only travel one angled-pixel at a time, we will ensure that we hit
		// every x/y pixel in between current and destination positions.
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

		// this whole system of returning results could use some cleanup, seems messy.
		// Tried using events, but the entire physics loop really needs to remain synchronous
		// to avoid hard to debug codepaths.
		PhysicsStepResult result = new PhysicsStepResult();

		for (float traveled = 0; traveled <= ballDistance; ) {
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
				result = DoHitChecks(_lastPixelBallPosition);
			}
		}

		return result;
	}

	private PhysicsStepResult DoHitChecks(Vector2Int ballPos) {
		var result = new PhysicsStepResult();

		// check if below paddle
		if (ballPos.y <= Consts.PADDLE_Y) {
			_ballAlive = false;

			result.LifeLost = true;
			return result;
		}

		bool hitBrick = false;
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

		// Check all the bricks for hits
		for (int row = 0; row < Consts.NUM_BRICK_ROWS; row++) {
			var thisRow = _bricks[row];
			var rowY = Consts.BRICKS_START_Y + row * Consts.BRICK_HEIGHT;
			if (ballPos.y >= rowY && ballPos.y < rowY + Consts.BRICK_HEIGHT) {
				// check this row
				int colIndex = ballPos.x / Consts.BRICK_WIDTH;

				// if we have a brick, reflect
				// TODO: This method is called "CHECK brick hit" but modifies velocity,
				// should really clean that up
				if (thisRow[colIndex] && CheckBrickHit(thisRow, ballPos, colIndex)) {
					hitBrick = true;
					SoundManager.PlaySound(string.Format(Consts.SOUND_RESOURCE_BRICK_TEMPLATE, row));
					thisRow[colIndex] = false;
					_numRemainingBricks--;

					if (_numRemainingBricks == 0)
					{
						result.WonLevel = true;
					}

					result.ScoreChange += Consts.BRICK_VALUES[row];
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
		// "DVD Screensaver" if they hit top and side at exact same time

		// Play appropriate sound, but don't play multiple.  If we already hit a brick, skip,
		// because we played sound for it already.
		if (!hitBrick)
		{
			if (hitTop || hitSide)
			{
				SoundManager.PlaySound(Consts.SOUND_RESOURCE_WALL);
			}
			else if (hitPaddle)
			{
				SoundManager.PlaySound(Consts.SOUND_RESOURCE_PADDLE);
			}
		}

		return result;
	}

	private bool CheckBrickHit(BitArray rowBricks, Vector2 ballPos, int column) {
		// this is a little inaccurate, as we'll count a hit from below on the end
		// as a side hit.  This causes some really weird behavior sometimes.
		// If I had more time, I'd probably strip this out and just use some flavor
		// of Box2D, but that carries it's own set of problems.  For the given
		// timeframe, I suppose this is ok, but I'd love to improved it by trackig
		// last position, and seeing which angle it came in from.  But hey, it's no
		// weirder than the original Breakout, where the ball constantly warped
		// through entire bricks.
		int leftEdge = column * Consts.BRICK_WIDTH;
		int rightEdge = leftEdge + Consts.BRICK_WIDTH - 1;

		// short circuit evaluation should protect us from checking rowBricks[-1];
		bool hasLeftNeighbor = column == 0 || rowBricks[column - 1];
		bool hasRightNeighbor = column == Consts.BRICKS_PER_ROW - 1 || rowBricks[column + 1];

		if (ballPos.x >= leftEdge && ballPos.x <= rightEdge)
		{
			// if it was hit on the left, from the left, or on the
			// right, from the right, rebound x
			if ((ballPos.x == leftEdge && _ballVelocity.x > 0 && !hasLeftNeighbor) ||
				(ballPos.x == rightEdge && _ballVelocity.x < 0 && !hasRightNeighbor))
			{
				_ballVelocity.x *= -1;
			}
			else
			{
				// assume hit from above or below, rebound y
				_ballVelocity.y *= -1;
			}

			return true;
		}

		return false;
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
