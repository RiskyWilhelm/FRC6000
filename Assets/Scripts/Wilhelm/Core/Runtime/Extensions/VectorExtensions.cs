using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public static class VectorExtensions
{
	/// <returns> Angle in radians in PI (-180~180 Degrees) </returns>
	public static float ToAngle(this Vector2 a)
		=> MathF.Atan2(a.y, a.x);

	/// <returns> Angle in radians in 2PI (0~360 Degrees) </returns>
	public static float ToAngle_360(this Vector2 a)
		=> MathfUtils.Atan2_360(a.y, a.x);

	public static Vector2 Rotate(this Vector2 a, float rotateAngleInDegrees)
	{
		var radRotateAngle = rotateAngleInDegrees * Mathf.Deg2Rad;
		return new Vector2(
			a.x * Mathf.Cos(radRotateAngle) - a.y * Mathf.Sin(radRotateAngle),
			a.x * Mathf.Sin(radRotateAngle) + a.y * Mathf.Cos(radRotateAngle)
		);
	}

	public static Vector3 Rotate(this Vector3 a, float rotateAngleInDegrees, Vector3 axisToRotateAround)
	{
		return Quaternion.AngleAxis(rotateAngleInDegrees, axisToRotateAround) * a;
	}

	public static bool TryGetNearestVector(this Vector4 relativeTo, IEnumerable<Vector4> vectorEnumerable, out Vector4 nearestVector, Predicate<Vector4> predicateNearest = null)
	{
		var isFoundNearest = false;
		nearestVector = default;

		if (vectorEnumerable.Count() == 0)
			return isFoundNearest;

		float nearestHorizontalDistance = (vectorEnumerable.First() - relativeTo).sqrMagnitude;
		float iteratedDistance;

		// Check sqr distances and select nearest chicken
		foreach (var iteratedVector in vectorEnumerable)
		{
			iteratedDistance = (iteratedVector - relativeTo).sqrMagnitude;

			if ((iteratedDistance <= nearestHorizontalDistance) && (predicateNearest == null || predicateNearest.Invoke(iteratedVector)))
			{
				nearestVector = iteratedVector;
				nearestHorizontalDistance = iteratedDistance;
				isFoundNearest = true;
			}
		}

		return isFoundNearest;
	}

	public static bool TryGetNearestVector(this Vector2 relativeTo, IEnumerable<Vector2> vectorEnumerable, out Vector2 nearestVector, Predicate<Vector2> predicateNearest = null)
	{
		var isFoundNearest = false;
		nearestVector = default;

		if (vectorEnumerable.Count() == 0)
			return isFoundNearest;

		float nearestHorizontalDistance = (vectorEnumerable.First() - relativeTo).sqrMagnitude;
		float iteratedDistance;

		// Check sqr distances and select nearest chicken
		foreach (var iteratedVector in vectorEnumerable)
		{
			iteratedDistance = (iteratedVector - relativeTo).sqrMagnitude;

			if ((iteratedDistance <= nearestHorizontalDistance) && (predicateNearest == null || predicateNearest.Invoke(iteratedVector)))
			{
				nearestVector = iteratedVector;
				nearestHorizontalDistance = iteratedDistance;
				isFoundNearest = true;
			}
		}

		return isFoundNearest;
	}

	public static bool TryGetNearestVector(this Vector3 relativeTo, IEnumerable<Vector3> vectorEnumerable, out Vector3 nearestVector, Predicate<Vector3> predicateNearest = null)
	{
		var isFoundNearest = false;
		nearestVector = default;

		if (vectorEnumerable.Count() == 0)
			return isFoundNearest;

		float nearestHorizontalDistance = (vectorEnumerable.First() - relativeTo).sqrMagnitude;
		float iteratedDistance;

		// Check sqr distances and select nearest chicken
		foreach (var iteratedVector in vectorEnumerable)
		{
			iteratedDistance = (iteratedVector - relativeTo).sqrMagnitude;

			if ((iteratedDistance <= nearestHorizontalDistance) && (predicateNearest == null || predicateNearest.Invoke(iteratedVector)))
			{
				nearestVector = iteratedVector;
				nearestHorizontalDistance = iteratedDistance;
				isFoundNearest = true;
			}
		}

		return isFoundNearest;
	}
}