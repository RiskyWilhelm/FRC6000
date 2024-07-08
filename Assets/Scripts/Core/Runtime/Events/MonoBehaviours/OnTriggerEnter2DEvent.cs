using UnityEngine;
using UnityEngine.Events;

public sealed partial class OnTriggerEnter2DEvent : EventBase<UnityEvent<Collider2D>>
{
	// Update
	private void OnTriggerEnter2D(Collider2D collision)
	{
		raised?.Invoke(collision);
	}
}


#if UNITY_EDITOR

public sealed partial class OnTriggerEnter2DEvent
{ }

#endif