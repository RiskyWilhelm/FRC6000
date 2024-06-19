using System;
using UnityEngine;

public sealed partial class AppStateControllerSingleton : MonoBehaviourSingletonBase<AppStateControllerSingleton>
{
	#region Javascript States

	[field: NonSerialized]
	public static JSVisibilityStateType VisibilityState { get; private set; }

	[field: NonSerialized]
	public static bool IsQuitting { get; private set; }

	#endregion


	// Initialize
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInitialization()
	{
		if (!IsInstanceLiving)
			FindOrCreate();

		Debug.LogFormat("Initialized the bridge GameObject of JS>C# named '{0}'", Instance.GameObjectName);
	}

	protected override void Awake()
	{
		DontDestroyOnLoad(Instance);
		base.Awake();
	}


	// Update
	private void OnVisibilityChange(string value) => VisibilityState = Enum.Parse<JSVisibilityStateType>(value, true);

	private void OnBeforeUnload() => IsQuitting = true;

	// TODO: In mobile, this should act like OnBeforeUnload. See: https://www.igvita.com/2015/11/20/dont-lose-user-and-app-state-use-page-visibility/
	private void OnPageHide(int isPersisted) => IsQuitting = true;
}


#if UNITY_EDITOR

public sealed partial class AppStateControllerSingleton
{
	private void OnApplicationPause(bool pause)
	{
		if (pause)
			VisibilityState = JSVisibilityStateType.Hidden;
		else
			VisibilityState = JSVisibilityStateType.Visible;
	}

	private void OnApplicationQuit()
	{
		IsQuitting = true;
	}
}

#endif