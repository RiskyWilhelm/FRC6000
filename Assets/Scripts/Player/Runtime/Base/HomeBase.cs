using UnityEngine;

[DisallowMultipleComponent]
public abstract partial class HomeBase : MonoBehaviour
{
	// Update
	public virtual bool TrySpawn(out AIBase spawnedAI)
	{
		spawnedAI = null;
		return false;
	}
}


#if UNITY_EDITOR

public abstract partial class HomeBase
{ }

#endif