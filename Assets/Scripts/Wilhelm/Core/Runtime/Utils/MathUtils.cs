using System;
using UnityEngine;

public static class MathfUtils
{
	/// <summary> Returns the angle in 2PI radians whose Tan is y/x. Same as <see cref="Mathf.Atan2(float, float)"/> but returns in 2PI radians (0-360 degree) instead of PI radians (-180~180 degree) </summary>
	public static float Atan2_360(float y, float x)
	{
		var radian = MathF.Atan2(y, x);
		
		// Same as adding 360 degree to a angle in degrees
		if (radian < 0f)
			radian += (Mathf.PI * 2);

		return radian;
	}

	public static float InvertAngle_360(float angleInDegrees)
	{
		return (angleInDegrees + (180 * Math.Sign(angleInDegrees))) % 360;
	}

	public static float InvertAngle(float angleInRadians)
	{
		return (angleInRadians + (Mathf.PI * Math.Sign(angleInRadians))) % (Mathf.PI * 2);
	}
}