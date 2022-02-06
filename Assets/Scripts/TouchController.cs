using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchController : IController
{
	// even if there are no touches, we need to return a value, so store
	// the last touch point
	private float _lastTouch = .5f;

	public float GetNewPaddleLeftRatio(Physics p)
	{
		#if UNITY_EDITOR
		Vector3 mousePos = Input.mousePosition;
		_lastTouch = (float)mousePos.x / Screen.width;
		return _lastTouch;
		#else
		if (Input.touchCount > 0)
		{
			// multi-touch is a bit more complex, and out of scope for now.
			Touch touch = Input.GetTouch(0);
			var leftRatio = (float)touch.position.x / (float)Screen.width;

			// if we're running landscape, we really don't want to make the user
			// drag their finger all over the screen.  Only use the middle 50% of
			// the screen. 0 to .25 becomes 0. .5 stays .5.  .75 and up becomes 1.
			leftRatio = Mathf.Min(1, Mathf.Max(0, leftRatio - .25f) * 2f);
			_lastTouch = leftRatio;
		}

		return _lastTouch;
		#endif
	}
}
