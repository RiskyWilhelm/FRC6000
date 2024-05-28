using UnityEngine;

[DisallowMultipleComponent]
public sealed partial class EventReflector : MonoBehaviour
{
    public GameObject reflectTo;

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
		if (!TryGetReflectedGameObject(searchGameObject, out GameObject foundGameObject))
			foundGameObject = searchGameObject;

		// Try Get AI target
		return foundGameObject.TryGetComponent<TargetType>(out foundTarget);
	}
}


#if UNITY_EDITOR

public sealed partial class EventReflector
{ }

#endif