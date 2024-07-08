using UnityEngine;
using UnityEngine.Events;

public sealed partial class OnTriggerStay2DEvent : EventBase<UnityEvent<Collider2D>>
{
	// Update
	private void OnTriggerStay2D(Collider2D collision)
	{
		raised?.Invoke(collision);
	}
}


#if UNITY_EDITOR

public sealed partial class OnTriggerStay2DEvent
{ }

#endif