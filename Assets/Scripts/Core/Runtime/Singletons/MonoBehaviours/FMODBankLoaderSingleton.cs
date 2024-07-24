using FMODUnity;
using System.Collections.Generic;

public sealed partial class FMODBankLoaderSingleton : MonoBehaviourSingletonBase<FMODBankLoaderSingleton>
{
	[BankRef]
	public List<string> Banks;

	public bool PreloadSamples;


	// Initialize
	private void OnEnable()
	{
		Load();
	}


	// Update
	public void Load()
	{
		foreach (var bankRef in Banks)
		{
			try
			{
				RuntimeManager.LoadBank(bankRef, PreloadSamples);
			}
			catch (BankLoadException e)
			{
				RuntimeUtils.DebugLogException(e);
			}
		}
		RuntimeManager.WaitForAllSampleLoading();
	}

	public void Unload()
	{
		foreach (var bankRef in Banks)
			RuntimeManager.UnloadBank(bankRef);
	}


	// Dispose
	private void OnDisable()
	{
		if (GameControllerPersistentSingleton.IsQuitting)
			return;

		Unload();
	}
}


#if UNITY_EDITOR

public sealed partial class FMODBankLoaderSingleton
{ }

#endif