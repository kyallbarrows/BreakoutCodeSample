using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : IController
{
	/// <summary>
	/// Calculates a number between 0 (inclusive) and 1 (inclusive), indicating
	/// how far from left paddle should be within it's allowed range of motion.
	/// 0 is full left, 1 is full right.
	/// </summary>
	/// <param name="p">An instance of class Physics, sometimes needed to do
	/// math on touch position vs paddle position, or for an AI player</param>
	/// <returns>A number between 0 (inclusive) and 1 (inclusive)</returns>
	public float GetNewPaddleLeftRatio(Physics p)
	{
		throw new System.NotImplementedException();
	}
}
