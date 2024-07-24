using FMODUnity;
using UnityEngine;

public sealed partial class FMODForcePlayEventEmitter : MonoBehaviour
{
	[SerializeField]
	private StudioEventEmitter eventEmitter;

	[SerializeField]
	private bool destroyAfterPlay;


	// Update
	private void Update()
	{
		if (eventEmitter.EventInstance.IsPlaying())
		{
			if (destroyAfterPlay)
				Destroy(this);
		}
		else
			eventEmitter.Play();
	}
}


#if UNITY_EDITOR

public sealed partial class FMODForcePlayEventEmitter
{ }

#endif