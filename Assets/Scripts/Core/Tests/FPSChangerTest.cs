using UnityEngine;

public sealed partial class FPSChangerTest : MonoBehaviour
{
    public int fps = 60;

	// Initialize


	// Update
	private void Update()
	{
		if (Application.targetFrameRate != fps)
			Application.targetFrameRate = fps;
	}


	// Dispose
}


#if UNITY_EDITOR

public sealed partial class FPSChanger
{ }

#endif