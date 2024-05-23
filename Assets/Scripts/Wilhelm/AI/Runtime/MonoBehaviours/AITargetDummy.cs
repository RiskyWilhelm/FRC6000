using UnityEngine;

public sealed partial class AITargetDummy : MonoBehaviour, IAITarget
{
	public AIBase ownerAI;

	public Vector2 Position => this.transform.position;

	public float OthersMaxApproachDistance => 0;

	public byte Power => 0;


	// Update
	private void Update()
	{
		if (ownerAI.currentTarget.UnderlyingValue != this)
			AITargetDummyPool.Release(this);
	}

	public void OnGotCaughtBy(AIBase chaser)
	{
		chaser.ClearTarget();
		AITargetDummyPool.Release(this);
	}
}


#if UNITY_EDITOR

public sealed partial class AITargetDummy
{ }

#endif