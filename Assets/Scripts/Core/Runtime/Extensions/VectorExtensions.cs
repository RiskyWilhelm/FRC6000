using System.Collections.Generic;
using System;
using UnityEngine;

public static class VectorExtensions
{
	/// <returns> Angle in radians in PI (-180~180 Degrees) </returns>
	public static float ToRadianAngle(this Vector2 a)
		=> MathF.Atan2(a.y, a.x);

	/// <returns> Angle in degrees in PI (-180~180) </returns>
	public static float ToDegreeAngle(this Vector2 a)
		=> MathF.Atan2(a.y, a.x) * Mathf.Rad2Deg;

	/// <returns> Angle in radians in 2PI (0~360 Degrees) </returns>
	public static float ToRadianAngle_360(this Vector2 a)
	{
		var radian = MathF.Atan2(a.y, a.x);

		// Same as adding 360 degree to a angle in degrees
		if (radian < 0f)
			radian += (Mathf.PI * 2);

		return radian;
	}

	/// <returns> Angle in degrees in 2PI (0~360) </returns>
	public static float ToDegreeAngle_360(this Vector2 a)
		=> ToRadianAngle_360(a) * Mathf.Rad2Deg;

	/// <returns> Non-normalized rotated original vector </returns>
	public static Vector2 RotateByRadianAngle(this Vector2 a, float rotateAngleInRadians)
	{
		return new Vector2(
			a.x * MathF.Cos(rotateAngleInRadians) - a.y * MathF.Sin(rotateAngleInRadians),
			a.x * MathF.Sin(rotateAngleInRadians) + a.y * MathF.Cos(rotateAngleInRadians)
		);
	}

	/// <returns> Non-normalized rotated original vector </returns>
	public static Vector3 RotateByRadianAngle(this Vector3 a, float rotateAngleInRadians, Vector3 axisToRotateAround)
	{
		return Quaternion.AngleAxis(rotateAngleInRadians * Mathf.Rad2Deg, axisToRotateAround) * a;
	}

	/// <returns> Non-normalized rotated original vector </returns>
	public static Vector2 RotateByDegreeAngle(this Vector2 a, float rotateAngleInDegrees)
		=> RotateByRadianAngle(a, rotateAngleInDegrees * Mathf.Deg2Rad);

	public static Vector3 RotateByDegreeAngle(this Vector3 a, float rotateAngleInDegrees, Vector3 axisToRotateAround)
		=> RotateByRadianAngle(a, rotateAngleInDegrees * Mathf.Deg2Rad, axisToRotateAround);

	public static bool TryGetNearestVector<VectorEnumeratorType>(this Vector4 relativeTo, VectorEnumeratorType vectorCollection, out Vector4 nearestVector, Predicate<Vector4> predicateNearest = null)
		where VectorEnumeratorType : IEnumerator<Vector4>
	{
		nearestVector = default;

		var isFoundNearest = false;
		float nearestHorizontalDistance = float.MaxValue;
		float iteratedDistance;

		// Check sqr distances and select nearest chicken
		foreach (var iteratedVector in vectorCollection)
		{
			iteratedDistance = (iteratedVector - relativeTo).sqrMagnitude;

			if ((iteratedDistance < nearestHorizontalDistance) && (predicateNearest == null || predicateNearest.Invoke(iteratedVector)))
			{
				nearestVector = iteratedVector;
				nearestHorizontalDistance = iteratedDistance;
				isFoundNearest = true;
			}
		}

		return isFoundNearest;
	}

	public static bool TryGetNearestVector<VectorEnumeratorType>(this Vector2 relativeTo, VectorEnumeratorType vectorCollection, out Vector2 nearestVector, Predicate<Vector2> predicateNearest = null)
		where VectorEnumeratorType : IEnumerator<Vector2>
	{
		nearestVector = default;

		var isFoundNearest = false;
		float nearestHorizontalDistance = float.MaxValue;
		float iteratedDistance;

		// Check sqr distances and select nearest chicken
		foreach (var iteratedVector in vectorCollection)
		{
			iteratedDistance = (iteratedVector - relativeTo).sqrMagnitude;

			if ((iteratedDistance < nearestHorizontalDistance) && (predicateNearest == null || predicateNearest.Invoke(iteratedVector)))
			{
				nearestVector = iteratedVector;
				nearestHorizontalDistance = iteratedDistance;
				isFoundNearest = true;
			}
		}

		return isFoundNearest;
	}

	public static bool TryGetNearestVector<VectorEnumeratorType>(this Vector3 relativeTo, VectorEnumeratorType vectorCollection, out Vector3 nearestVector, Predicate<Vector3> predicateNearest = null)
		where VectorEnumeratorType : IEnumerator<Vector3>
	{
		nearestVector = default;

		var isFoundNearest = false;
		float nearestHorizontalDistance = float.MaxValue;
		float iteratedDistance;

		// Check sqr distances and select nearest chicken
		foreach (var iteratedVector in vectorCollection)
		{
			iteratedDistance = (iteratedVector - relativeTo).sqrMagnitude;

			if ((iteratedDistance < nearestHorizontalDistance) && (predicateNearest == null || predicateNearest.Invoke(iteratedVector)))
			{
				nearestVector = iteratedVector;
				nearestHorizontalDistance = iteratedDistance;
				isFoundNearest = true;
			}
		}

		return isFoundNearest;
	}
}