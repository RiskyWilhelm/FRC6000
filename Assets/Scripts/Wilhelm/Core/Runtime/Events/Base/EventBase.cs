using UnityEngine;
using UnityEngine.Events;

public abstract partial class EventBase<EventType> : MonoBehaviour
    where EventType : UnityEventBase
{
    public EventType raised;
}


#if UNITY_EDITOR

public abstract partial class EventBase<EventType>
{ }

#endif