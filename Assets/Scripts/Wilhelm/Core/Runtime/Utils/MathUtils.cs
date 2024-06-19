using UnityEngine;

public static class MathfUtils
{
	/// <summary> Same as <see cref="Mathf.Atan2(float, float)"/> but returns in 2PI radians (360 degree) instead of PI radians (180 degree) </summary>
	public static float Atan2_360(float y, float x)
	{
		var radian = Mathf.Atan2(y, x);
		
		// Same as adding 360 degree to a angle in degrees
		if (radian < 0f)
			radian += (Mathf.PI * 2);

		return radian;
	}
}