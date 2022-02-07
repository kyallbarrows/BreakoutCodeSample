using System;
using System.Collections;
using UnityEngine;

public class Breakout : MonoBehaviour
{
    // the texture we'll be drawing Breakout into
    private Texture2D _mainTex;

    private DrawBuffer _drawBuffer;
    private Physics _physics;
    private IController _currentGameController;
    private AIController _aiController;
    private TouchController _touchController;
    private bool _useAIController = true;

    private int _score = 0;
    private int _lives = Consts.INITIAL_LIVES;
    private int _gameLevel = 1;

    // Start is called before the first frame update
    void Start()
    {
        try {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

            // camera is in fixed position relative to tv screen, and texture
            // needs to be an exact pixel size, so turn off mip maps, we won't
            // need them.
            _mainTex = new Texture2D(128, 128, TextureFormat.RGB24, false);
            meshRenderer.material.mainTexture = _mainTex;
        }
        catch(Exception e) {
            Debug.LogError($"Breakout: unable to get all or part of MeshRenderer's main texture: {e.Message} ({e.GetType()})");
        }

        _physics = new Physics();
        _drawBuffer = new DrawBuffer(_mainTex);

        // create both touch and AI controllers so we can swap back and forth
        _touchController = new TouchController();
        _aiController = new AIController();

        // pre-load all the sounds into memory. They're tiny, so it's ok.
        SoundManager.Init(this.gameObject);

        // start the story playing in the background
        SoundManager.PlaySound(Consts.SOUND_RESOURCE_STORY);

        // always good to begin with a reset
        ResetLevel(_gameLevel);
    }

    /// <summary>
	/// Resets the level to the initial state, rewinding the script, audio,
	/// resetting game score and draw buffer, etc
	/// </summary>
	/// <param name="level">The level, 1-N, to reset to.</param>
    public void ResetLevel(int level)
    {
        // level is 1-indexed, so subtract 1 from it before calc'ing new speed
        float thisLevelBallSpeed = Consts.DEFAULT_BALL_SPEED +
            (level - 1) * Consts.BALL_SPEED_INCREASE_PER_LEVEL;
        _physics.Reset(thisLevelBallSpeed);
        _physics.ResetBall();
    }

    void Update()
    {
        // run a frame.
        // Unlike original Breakout, where execution was tied directly to TV
        // sync, we need to check how much time has elapsed between frames, so
        // get time since last frame render
        // TODO: this needs to use a pausable clock, so the game is pausable.
        float deltaT = Time.deltaTime;

        // get touch input, or AI player input, and move paddle accordingly
        _currentGameController = _useAIController ? (IController)_aiController : (IController)_touchController;
        var newPaddleLeftRatio = _currentGameController.GetNewPaddleLeftRatio(_physics);
        _physics.MovePaddleTo(newPaddleLeftRatio);

        // run physics step
        var result = _physics.RunStep(deltaT);
        _score += result.ScoreChange;

        // repaint the TV screen
        DoDrawCycle();

        if (result.LifeLost)
        {
            _lives--;
            _physics.ResetBall();

            if (_lives == 0)
            {
                // not really sure what should happen here, and we're running short on time.
                // for now, just reset to level 1 and restart game
                _lives = Consts.INITIAL_LIVES;
                _gameLevel = 1;
                ResetLevel(_gameLevel);
            }
        }
        else if (result.WonLevel) {
            // just bump the level and reset
            _gameLevel++;
            ResetLevel(_gameLevel);
        }
    }

    /// <summary>
	/// Redraws the score bar, paddle, bricks, and ball
	/// </summary>
    private void DoDrawCycle() {
        _drawBuffer.BeginDrawCycle();

        // draw score/lives/level
        // cap the score for now...I'm sure the original handled it somehow.
        int thisScore = Mathf.Min(999, _score);
        int ones = thisScore % 10;
        int tens = ((thisScore - ones) % 100) / 10;
        int hundreds = thisScore / 100;
        _drawBuffer.DrawNumber(hundreds, Consts.SCORE_X, Consts.STATS_Y);
        _drawBuffer.DrawNumber(tens, Consts.SCORE_X + Consts.TOTAL_DIGIT_WIDTH, Consts.STATS_Y);
        _drawBuffer.DrawNumber(ones, Consts.SCORE_X + Consts.TOTAL_DIGIT_WIDTH * 2, Consts.STATS_Y);

        _drawBuffer.DrawNumber(_lives, Consts.LIVES_X, Consts.STATS_Y);
        _drawBuffer.DrawNumber(_gameLevel, Consts.LEVEL_X, Consts.STATS_Y);

        // draw all the bricks
        for (int row = 0; row < Consts.NUM_BRICK_ROWS; row++) {
            DrawBrickRow(_physics.GetBrickRow(row), row);
        }

        // draw paddle and ball
        _drawBuffer.DrawPaddle(_physics.PaddleLeftX, Consts.PADDLE_Y);
        _drawBuffer.DrawBall(_physics.PixelBallPosition.x, _physics.PixelBallPosition.y);

        // finalize and rebuild the texture
        _drawBuffer.FinishDrawCycle();
    }

    private void DrawBrickRow(BitArray brickStatuses, int rowIndex) {
        for (int col = 0; col < Consts.BRICKS_PER_ROW; col++) {
            if (brickStatuses[col]) {
                _drawBuffer.DrawBrick(col, rowIndex);
            }
        }
    }

    void OnGUI()
    {
        // So sorry for using OnGUI, it's just the worst.
        _useAIController = GUILayout.Toggle(_useAIController, "Autoplay");
    }
}
