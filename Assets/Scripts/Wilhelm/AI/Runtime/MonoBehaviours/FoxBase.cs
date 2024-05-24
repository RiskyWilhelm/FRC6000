using UnityEngine;

public sealed partial class FoxBase : MonoBehaviour, IAITarget
{
	public ushort Power => 0;

	public ushort Health => 0;


	private void Start()
	{
		Debug.Log("Start");
	}

	// Update
	public void OnGotAttackedBy(AIBase chaser)
	{ }
}


#if UNITY_EDITOR

public sealed partial class FoxBase
{ }

#endif