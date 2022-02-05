using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakout : MonoBehaviour
{
    private Texture2D _mainTex;
    private MeshRenderer _meshRenderer;

    private DrawBuffer _drawBuffer;
    private Physics _physics;
    private IController _gameController;

    private int _score = 0;
    private int _lives = Consts.INITIAL_LIVES;
    private int _gameLevel = 1;

    public bool AIControlEnabled = false;

    // Start is called before the first frame update
    void Start()
    {
        try {
            _meshRenderer = GetComponent<MeshRenderer>();
            // camera is in fixed position relative to tv screen, and texture
            // needs to be an exact pixel size, so turn off mip maps
            _mainTex = new Texture2D(128, 128, TextureFormat.RGB24, false);
            _meshRenderer.material.mainTexture = _mainTex;
        }
        catch(Exception e) {
            Debug.Log($"Breakout: unable to get all or part of MeshRenderer's main texture: {e.Message} ({e.GetType()})");
        }

        _drawBuffer = new DrawBuffer(_mainTex);

        ResetLevel(_gameLevel);
    }

    /// <summary>
	/// Resets the level to the initial state, rewinding the script, audio,
	/// resetting game score and draw buffer, etc
	/// </summary>
	/// <param name="level">The level, 1-N, to reset to.</param>
    public void ResetLevel(int level)
    {
        // TODO: need to do this or new AIController based on menu choice
        _gameController = new TouchController();

        // level is 1-indexed, so subtract 1 from it before calc'ing new speed
        float thisLevelBallSpeed = Consts.DEFAULT_BALL_SPEED +
            (level - 1) * Consts.BALL_SPEED_INCREASE_PER_LEVEL;
        _physics = new Physics(thisLevelBallSpeed);
    }

    private bool CheckForWin() {
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        // run a frame.
        // Unlike original Breakout, where execution was tied directly to TV
        // sync, we need to check how much time has elapsed between frames, so
        // get time since last frame render
        // TODO: this needs to use a pausable clock, so the game is pausable.
        float deltaT = Time.deltaTime;

        // get touch input, or AI player input, and move paddle accordingly

        // run physics step
        RunPhysicsStep(deltaT);
        DoDrawCycle();

        // TODO: move out to external portion of app
        // handle what's going on in the 3D scene
        // - check if we're on the next step of the script, and call any tasks
        // - turn on or off any oscillating lights
        // - update any tweens on gameobjects

        if (CheckForWin()) {
            _gameLevel++;
            ResetLevel(_gameLevel);
        }

        /*
        Color redColor = new Color(1f, 0.0f, 0.0f);
        Color greenColor = new Color(0.0f, 1f, 0.0f);
        Color blueColor = new Color(0.0f, 0.0f, 1f);
        var fillColorArray =  _mainTex.GetPixels();
        
        for(var i = 0; i < fillColorArray.Length; ++i)
        {
            if (i % 3 == 0) fillColorArray[i] = redColor;
            if (i % 3 == 1) fillColorArray[i] = greenColor;
            if (i % 3 == 2) fillColorArray[i] = blueColor;
        }
        
        _mainTex.SetPixels( fillColorArray );
        _mainTex.Apply();
        */
    }

    private void RunPhysicsStep(float deltaT) { }

    private void DoDrawCycle() {
        _drawBuffer.BeginDrawCycle();

        // - draw bricks
        // - draw ball
        // - draw paddle
        // decide if we want to do any vsync errors on the TV screen


        // - draw score/lives/level
        // score should never get above 800-ish, but limit just in case
        int thisScore = (int)Math.Min(0, _score);
        int hundreds = thisScore / 100;
        int tens = (thisScore - hundreds) / 10;
        int ones = thisScore - (hundreds + tens);
        _drawBuffer.DrawNumber(hundreds, Consts.SCORE_X, Consts.STATS_Y);
        _drawBuffer.DrawNumber(tens, Consts.SCORE_X + Consts.TOTAL_DIGIT_WIDTH, Consts.STATS_Y);
        _drawBuffer.DrawNumber(ones, Consts.SCORE_X + Consts.TOTAL_DIGIT_WIDTH * 2, Consts.STATS_Y);
        _drawBuffer.DrawNumber(_lives, Consts.LIVES_X, Consts.STATS_Y);
        _drawBuffer.DrawNumber(_gameLevel, Consts.LEVEL_X, Consts.STATS_Y);

        _drawBuffer.DrawPaddle(_physics.PaddleLeftX, Consts.PADDLE_Y);
        _drawBuffer.DrawBall(20, 20);

        _drawBuffer.FinishDrawCycle();
    }
}
