using System.Collections.Generic;
using UnityEngine;

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

	public static bool TryGetActiveObjectListFromTag(string checkTag, out List<Transform> activeTagObjectList)
	{
		// Return the copy so we dont need to worry about modifying the list
		if (IsTagHaveLivingObjects(checkTag))
		{
			activeTagObjectList = new List<Transform>(activeTagObjectDictionary[checkTag]);
			return true;
		}

		activeTagObjectList = null;
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