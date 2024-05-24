using UnityEngine;

public sealed partial class AITargetDummy : MonoBehaviour, IAITarget
{
	public byte Power => 0;


	// Update
	public void OnGotAttacked(AIBase chaser)
	{
		AITargetDummyPool.Release(this);
	}
}


#if UNITY_EDITOR

public sealed partial class AITargetDummy
{ }

#endif