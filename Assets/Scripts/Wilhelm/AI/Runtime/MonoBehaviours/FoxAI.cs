using UnityEngine;

public sealed partial class FoxAI : AIBase
{
	protected override void Update()
	{
		this.SetDestination(GameObject.FindFirstObjectByType<ChickenAI>().transform.position);
		base.Update();
	}
}


#if UNITY_EDITOR

public sealed partial class FoxAI
{ }

#endif