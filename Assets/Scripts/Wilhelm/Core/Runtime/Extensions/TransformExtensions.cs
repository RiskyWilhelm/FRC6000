using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TransformExtensions
{
	public static bool TryGetNearestTransform(this Transform relativeTo, IEnumerable<Transform> transformEnumerable, out Transform nearestTransform, Predicate<Transform> predicateNearest = null)
	{
		var isFoundNearest = false;
		nearestTransform = default;

		if (transformEnumerable.Count() == 0)
			return isFoundNearest;

		float nearestHorizontalDistance = (transformEnumerable.First().position - relativeTo.position).sqrMagnitude;
		float iteratedDistance;

		// Check sqr distances and select nearest chicken
		foreach (var iteratedTransform in transformEnumerable)
		{
			iteratedDistance = (iteratedTransform.position - relativeTo.position).sqrMagnitude;

			if ((iteratedDistance <= nearestHorizontalDistance) && (predicateNearest == null || predicateNearest.Invoke(iteratedTransform)))
			{
				nearestTransform = iteratedTransform;
				nearestHorizontalDistance = iteratedDistance;
				isFoundNearest = true;
			}
		}

		return isFoundNearest;
	}
}
