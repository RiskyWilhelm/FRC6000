using System;
using UnityEngine;

public static class VectorUtils
{
	public static Vector2 AngleToVector(float angleInRadians)
	{
		return new Vector2(MathF.Cos(angleInRadians), MathF.Sin(angleInRadians));
	}
}
