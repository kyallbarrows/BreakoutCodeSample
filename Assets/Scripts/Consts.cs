public class Consts
{
	public const int NUM_BRICK_ROWS = 6;
	public const int BRICKS_PER_ROW = 18;
	public const float DEFAULT_BALL_SPEED = 12f;
	public const int INITIAL_LIVES = 5;

	// not sure how quickly things ramped up on the original breakout, but this might work
	public const float BALL_SPEED_INCREASE_PER_LEVEL = 2f;

	// these are the actual dimensions we'll be drawing into. 
	public const int GAME_WIDTH = 100;
	public const int GAME_HEIGHT = 62;

	public const int SCORE_X = 22;
	public const int LIVES_X = 63;
	public const int LEVEL_X = 78;
	public const int STATS_Y = GAME_HEIGHT - 5;

	public static readonly byte[] GRAY = { 0x99, 0x99, 0x99 };
	public static readonly byte[] BLUE = { 0x00, 0x00, 0xFF };
	public static readonly byte[] GREEN = { 0x00, 0xFF, 0x00 };
	public static readonly byte[] YELLOW = { 0x99, 0x99, 0x00 };
	public static readonly byte[] ORANGE = { 0x99, 0x99, 0x00 };
	public static readonly byte[] DEEP_ORANGE = { 0x99, 0x99, 0x00 };
	public static readonly byte[] RED = { 0xFF, 0x00, 0x00 };

	public const int BALL_WIDTH = 1;
	public const int BALL_HEIGHT = 1;
	public const int PADDLE_WIDTH = 10;
	public const int PADDLE_HEIGHT = 1;
	public const int PADDLE_Y = 1;
	public const int BRICK_WIDTH = 5;
	public const int BRICK_HEIGHT = 2;
	public const int HOUSE_WIDTH = 100;
	public const int HOUSE_HEIGHT = 56;
	public const int HOUSE_WALL_THICKNESS = 5; // how far from the side of the scene we need to draw the ball, bricks, paddle
	public const int DIGIT_WIDTH = 9;
	public const int DIGIT_HEIGHT = 5;
	public const int TOTAL_DIGIT_WIDTH = 10;	// the width including spacing

	public const string SPRITE_HOUSE = "Sprites/BreakoutScreenElements/house";
	public const string SPRITE_PADDLE = "Sprites/BreakoutScreenElements/paddle";
	public const string SPRITE_BALL = "Sprites/BreakoutScreenElements/ball";
	public const string SPRITE_DIGIT_TEMPLATE = "Sprites/BreakoutScreenElements/digit{0}";

	public const string SPRITE_BRICK_TEMPLATE = "brickForRow{0}";
	public const string SPRITE_BRICK_RED = "Sprites/BreakoutScreenElements/brickRed";
	public const string SPRITE_BRICK_BROWN = "Sprites/BreakoutScreenElements/brickBrown";
	public const string SPRITE_BRICK_ORANGE = "Sprites/BreakoutScreenElements/brickOrange";
	public const string SPRITE_BRICK_YELLOW = "Sprites/BreakoutScreenElements/brickYellow";
	public const string SPRITE_BRICK_GREEN = "Sprites/BreakoutScreenElements/brickGreen";
	public const string SPRITE_BRICK_BLUE = "Sprites/BreakoutScreenElements/brickBlue";
}
