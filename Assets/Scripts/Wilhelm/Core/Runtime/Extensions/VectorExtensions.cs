using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public static class VectorExtensions
{
	public static bool TryGetNearestVector(this Vector4 relativeTo, IEnumerable<Vector4> vectorEnumerable, out Vector4 nearestVector, Predicate<Vector4> predicateNearest = null)
	{
		var isFoundNearest = false;
		nearestVector = default;

		if (vectorEnumerable.Count() == 0)
			return isFoundNearest;

		float nearestHorizontalDistance = (vectorEnumerable.First() - relativeTo).sqrMagnitude;

		// Check sqr distances and select nearest chicken
		foreach (var iteratedVector in vectorEnumerable)
		{
			var iteratedDistance = (iteratedVector - relativeTo).sqrMagnitude;

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

		// Check sqr distances and select nearest chicken
		foreach (var iteratedVector in vectorEnumerable)
		{
			var iteratedDistance = (iteratedVector - relativeTo).sqrMagnitude;

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

		// Check sqr distances and select nearest chicken
		foreach (var iteratedVector in vectorEnumerable)
		{
			var iteratedDistance = (iteratedVector - relativeTo).sqrMagnitude;

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