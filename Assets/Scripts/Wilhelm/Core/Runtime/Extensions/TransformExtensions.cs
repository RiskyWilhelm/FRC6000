using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public static class TransformExtensions
{
	public static bool TryGetNearestTransform(this Transform relativeTo, IEnumerable<Transform> transformEnumerable, out Transform nearestTransform, Predicate<Transform> predicateNearest = null)
	{
		nearestTransform = default;

		// Convert transformEnumerable to vectorEnumerable
		var cachedVectorDict = DictionaryPool<Vector3, Transform>.Get();

        foreach (var iteratedTransform in transformEnumerable)
			cachedVectorDict.TryAdd(iteratedTransform.position, iteratedTransform);

		// Get nearest if possible
		if (VectorExtensions.TryGetNearestVector(relativeTo.position, cachedVectorDict.Keys, out Vector3 nearestVector,
			(predicateNearest != null) ? (iteratedVector) => predicateNearest.Invoke(cachedVectorDict[iteratedVector]) : null))
		{
			nearestTransform = cachedVectorDict[nearestVector];
		}

		DictionaryPool<Vector3, Transform>.Release(cachedVectorDict);
		return nearestTransform != default;
	}
}
