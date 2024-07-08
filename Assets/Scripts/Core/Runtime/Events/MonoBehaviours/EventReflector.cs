using UnityEngine;

[DisallowMultipleComponent]
public sealed partial class EventReflector : MonoBehaviour
{
    public GameObject reflectTo;
}


#if UNITY_EDITOR

public sealed partial class EventReflector
{ }

#endif