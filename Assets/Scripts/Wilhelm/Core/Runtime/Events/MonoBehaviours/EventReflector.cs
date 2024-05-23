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

        reflectedTo = null;
        return false;
	}
}


#if UNITY_EDITOR

public sealed partial class EventReflector
{ }

#endif