using UnityEngine;

public sealed partial class ChickenAI : AIBase
{
	public override void OnGotCaughtBy(AIBase chaser)
	{
		Debug.LogFormat("Caught by {0}", chaser.name);
	}

	protected override void DoIdle()
	{
		
	}


	// Initialize


	// Update


	// Dispose
}


#if UNITY_EDITOR

public sealed partial class ChickenAI
{ }

#endif