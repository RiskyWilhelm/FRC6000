using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[DisallowMultipleComponent]
public sealed partial class TagObject : MonoBehaviour
{
	private static readonly Dictionary<string, List<Transform>> activeTagObjectDictionary = new ();


	// Initialize
	private void OnEnable()
	{
		// Try creating the tag list if there is none
		if (!IsTagHaveLivingObjects(tag))
			activeTagObjectDictionary[tag] = new List<Transform>();
		
		activeTagObjectDictionary[tag].Add(this.transform);
	}


	// Update
	public static bool IsTagHaveLivingObjects(string checkTag)
	{
		return activeTagObjectDictionary.ContainsKey(checkTag);
	}

	/// <summary> Gets the reference to the tag object list </summary>
	private static bool TryGetActiveObjectListFromTag(string checkTag, out List<Transform> activeTagObjectList)
	{
		// Return the copy so we dont need to worry about modifying the list
		if (IsTagHaveLivingObjects(checkTag))
		{
			activeTagObjectList = activeTagObjectDictionary[checkTag];
			return true;
		}

		activeTagObjectList = null;
		return false;
	}

	/// <summary> Gets the reference to the tag object list and converts to <see cref="ReadOnlyCollection{T}"/> </summary>
	public static bool TryGetActiveObjectListFromTag(string checkTag, out ReadOnlyCollection<Transform> activeTagObjectCollection)
	{
		if (TryGetActiveObjectListFromTag(checkTag, out List<Transform> activeTagObjectList))
		{
			activeTagObjectCollection = activeTagObjectList.AsReadOnly();
			return true;
		}
		
		activeTagObjectCollection = null;
		return false;
	}

	/// <param name="predicateNearest"> If the nearest chicken meets the criteria(predicate returns true), set it to nearest. If not, skip </param>
	public static bool TryGetNearestTagObject(Transform relativeTo, string checkTag, out Transform nearestTagObject, Predicate<Transform> predicateNearest = null)
	{
		nearestTagObject = null;

		if (TryGetActiveObjectListFromTag(checkTag, out List<Transform> activeTagObjectList))
		{
			// Ready
			float nearestHorizontalDistance = Mathf.Abs(activeTagObjectList[0].position.x - relativeTo.position.x);

			// Check distances and select nearest chicken
			foreach (var iteratedTagObject in activeTagObjectList)
			{
				var iteratedHorizontalDistance = Mathf.Abs(iteratedTagObject.position.x - relativeTo.position.x);

				if (iteratedHorizontalDistance <= nearestHorizontalDistance && (predicateNearest == null || predicateNearest.Invoke(iteratedTagObject)))
				{
					nearestTagObject = iteratedTagObject;
					nearestHorizontalDistance = iteratedHorizontalDistance;
				}
			}

			if (nearestTagObject != null)
				return true;
		}

		return false;
	}


	// Dispose
	private void OnDisable()
	{
		if (activeTagObjectDictionary[tag].Remove(this.transform) && (activeTagObjectDictionary[tag].Count == 0))
			activeTagObjectDictionary.Remove(tag);
	}
}


#if UNITY_EDITOR

public sealed partial class TagObject
{ }

#endif