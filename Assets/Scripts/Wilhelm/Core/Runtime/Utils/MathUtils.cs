using System;
using UnityEngine;

public static class MathfUtils
{
	/// <summary> Returns the angle in radians whose Tan is y/x. Same as <see cref="Mathf.Atan2(float, float)"/> but returns in 2PI radians (0-360 degree) instead of PI radians (-180~180 degree) </summary>
	public static float Atan2_360(float y, float x)
	{
		var radian = MathF.Atan2(y, x);
		
		// Same as adding 360 degree to a angle in degrees
		if (radian < 0f)
			radian += (Mathf.PI * 2);

		return radian;
	}
}