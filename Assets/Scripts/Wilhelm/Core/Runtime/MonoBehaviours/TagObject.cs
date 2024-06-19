using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Pool;

// TODO: This class does not support dynamic tag changes
[DisallowMultipleComponent]
public sealed partial class TagObject : MonoBehaviour
{
	// Stores tag> tagList, tagReadonlyList(connected to tagList)
	private static readonly Dictionary<string, ValueTuple<List<Transform>, ReadOnlyCollection<Transform>>> activeTagObjectDictionary = new();


	// Initialize
	private void OnEnable()
	{
		// Try creating the tag list if there is none
		if (!IsTagHaveLivingObjects(this.tag))
			CreateTagTuple();

		// Add to list
		activeTagObjectDictionary[tag].Item1.Add(this.transform);
	}

	private void CreateTagTuple()
	{
		var cachedTransformList = ListPool<Transform>.Get();

		activeTagObjectDictionary[this.tag] = new()
		{
			Item1 = cachedTransformList,
			Item2 = new(cachedTransformList)
		};
	}

	// TODO: Utils class should be created
	public static bool IsTagHaveLivingObjects(string checkTag) => activeTagObjectDictionary.ContainsKey(checkTag);

	/// <summary> Gets the reference to the tag object list and converts to <see cref="ReadOnlyCollection{T}"/> </summary>
	public static bool TryGetActiveObjectListFromTag(string checkTag, out ReadOnlyCollection<Transform> activeTagObjectReadonlyList)
	{
		activeTagObjectReadonlyList = null;

		if (IsTagHaveLivingObjects(checkTag))
			activeTagObjectReadonlyList = activeTagObjectDictionary[checkTag].Item2;

		return activeTagObjectReadonlyList != null;
	}

	/// <param name="predicateNearest"> If the nearest chicken meets the criteria(predicate returns true), set it to nearest. If not, skip </param>
	public static bool TryGetNearestTagObject(Transform relativeTo, string checkTag, out Transform nearestTagObject, Predicate<Transform> predicateNearest = null)
	{
		nearestTagObject = null;

		if (TryGetActiveObjectListFromTag(checkTag, out ReadOnlyCollection<Transform> activeTagObjectList))
			TransformExtensions.TryGetNearestTransform(relativeTo, activeTagObjectList, out nearestTagObject, predicateNearest);

		return nearestTagObject != null;
	}


	// Dispose
	private void OnDisable()
	{
		var activeTagObjectList = activeTagObjectDictionary[this.tag].Item1;

		if (activeTagObjectList.Remove(this.transform) && (activeTagObjectList.Count == 0))
		{
			activeTagObjectDictionary.Remove(this.tag, out var removedValue);
			ListPool<Transform>.Release(removedValue.Item1);
		}
	}
}


#if UNITY_EDITOR

public sealed partial class TagObject
{ }

#endif