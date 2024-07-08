using UnityEngine;
using UnityEngine.Events;

public sealed partial class OnTriggerExit2DEvent : EventBase<UnityEvent<Collider2D>>
{
	// Update
	private void OnTriggerExit2D(Collider2D collision)
	{
		raised?.Invoke(collision);
	}
}


#if UNITY_EDITOR

public sealed partial class OnTriggerExit2DEvent
{ }

#endif