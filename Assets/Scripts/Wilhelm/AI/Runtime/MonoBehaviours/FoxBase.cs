using UnityEngine;

public sealed partial class FoxBase : MonoBehaviour
{
	// Update
	public void OnFoxEnterToGate(Collider2D collider)
	{
		if (EventReflector.TryGetComponentByEventReflector<FoxAI>(collider.gameObject, out FoxAI gatePasser))
		{
			if (gatePasser.Stats.IsCaughtChicken)
				FoxAIPool.Release(gatePasser);
		}
	}
}


#if UNITY_EDITOR

public sealed partial class FoxBase
{ }

#endif