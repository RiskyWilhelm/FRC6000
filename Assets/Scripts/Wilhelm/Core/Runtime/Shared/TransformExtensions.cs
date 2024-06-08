using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TransformExtensions
{
	public static bool TryGetNearestTransform(this Transform relativeTo, IEnumerable<Transform> transformEnumerable, out Transform nearestTransform, Predicate<Transform> predicateNearest = null)
	{
		nearestTransform = null;

		if (transformEnumerable.Count() == 0)
			return false;

		float nearestHorizontalDistance = (transformEnumerable.First().position - relativeTo.position).sqrMagnitude;

		// Check sqr distances and select nearest chicken
		foreach (var iteratedTransform in transformEnumerable)
		{
			var iteratedDistance = (iteratedTransform.position - relativeTo.position).sqrMagnitude;

			if ((iteratedDistance <= nearestHorizontalDistance) && (predicateNearest == null || predicateNearest.Invoke(iteratedTransform)))
			{
				nearestTransform = iteratedTransform;
				nearestHorizontalDistance = iteratedDistance;
			}
		}

		return nearestTransform != null;
	}
}
