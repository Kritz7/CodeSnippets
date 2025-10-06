/*
* Author: Nicholas Blackburn
*
* A quick way to get AnimationCurve-style interpolation. 
* Calling GetCompletion(percentage,style) will return `percentage` as a value between 0..1 as if it were on a curve.
*
* Adapted from Robert Utter's "How to Lerp like a pro"
* source: https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
*/

using UnityEngine;
using System.Collections;

public class LerpAnimation : MonoBehaviour
{
	public enum LerpStyle
	{
		EaseIn,
		EaseOut,
		Smooth,
		SuperSmooth,
		Linear
	}
  
	public static float GetCompletion(float percentage, LerpStyle style)
	{
		switch(style)
		{
		case LerpStyle.EaseIn:
			return Mathf.Sin(percentage * Mathf.PI * 0.5f);
		case LerpStyle.EaseOut:
			return 1f - Mathf.Cos(percentage * Mathf.PI * 0.5f);
		case LerpStyle.Smooth:
			return percentage * percentage * (3f - 2f * percentage);
		case LerpStyle.SuperSmooth:
			return percentage * percentage * percentage * (percentage * (6f * percentage - 15f) + 10f);
		default:
			return percentage;
		}
	}
  
  public static float GetCompletion(float t, float duration, LerpStyle style)
	{
		return GetCompletion(t / duration, style);
	}
}
