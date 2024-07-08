using System;
using UnityEngine;

public static class VectorUtils
{
	public static Vector2 RadianToNormalizedVector(float angleInRadians)
	{
		return new Vector2(MathF.Cos(angleInRadians), MathF.Sin(angleInRadians));
	}

	public static Vector2 DegreeToNormalizedVector(float angleInDegrees)
		=> RadianToNormalizedVector(angleInDegrees * Mathf.Deg2Rad);
}
