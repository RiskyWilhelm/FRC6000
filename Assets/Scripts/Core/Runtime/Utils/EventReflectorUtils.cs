using System;
using UnityEngine;

public static class EventReflectorUtils
{
	/// <returns> If found, returns the reflected GameObject, else self </returns>
	public static bool TryGetReflectedGameObject(GameObject gameObject, out GameObject reflectedTo)
	{
		if (gameObject.TryGetComponent<EventReflector>(out EventReflector foundEventReflector))
		{
			reflectedTo = foundEventReflector.reflectTo;
			return true;
		}

		reflectedTo = gameObject;
		return false;
	}

	/// <summary> Same with <see cref="GameObject.TryGetComponent{T}(out T)"/> except it reflects the method to desired <see cref="GameObject"/> via <see cref="EventReflector"/> if there is any </summary>
	public static bool TryGetComponentByEventReflector<TargetType>(GameObject searchGameObject, out TargetType foundTarget)
	{
		// Check if event wants to reflect the collision. If there is no EventReflector, it is the main object that wants the event
		TryGetReflectedGameObject(searchGameObject, out searchGameObject);

		// Try Get AI target
		return searchGameObject.TryGetComponent<TargetType>(out foundTarget);
	}

	/// <summary> Same with <see cref="GameObject.TryGetComponent(System.Type, out Component)"/> except it reflects the method to desired <see cref="GameObject"/> via <see cref="EventReflector"/> if there is any </summary>
	public static bool TryGetComponentByEventReflector(Type targetType, GameObject searchGameObject, out Component foundTarget)
	{
		// Try get the reflected GameObject otherwise return same
		TryGetReflectedGameObject(searchGameObject, out searchGameObject);

		// Try Get AI target
		return searchGameObject.TryGetComponent(targetType, out foundTarget);
	}
}