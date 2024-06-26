using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class GameControllerSingleton : MonoBehaviourSingletonBase<GameControllerSingleton>
{
	#region GameControllerSingleton Javascript States

	[field: NonSerialized]
	public static JSVisibilityStateType VisibilityState { get; private set; }

	[field: NonSerialized]
	public static bool IsQuitting { get; private set; }

	#endregion

	#region GameControllerSingleton Events

	public readonly Dictionary<TargetType, Action> onTargetBirthDict = new();

	public readonly Dictionary<TargetType, Action> onTargetDeathDict = new();


	#endregion


	// Initialize
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInitialization()
	{
		if (!IsInstanceLiving)
			FindOrCreate();

        foreach (TargetType iteratedTargetType in Enum.GetValues(typeof(TargetType)))
        {
			Instance.onTargetBirthDict.Add(iteratedTargetType, new Action(() => { }));
			Instance.onTargetDeathDict.Add(iteratedTargetType, new Action(() => { }));
        }

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

public sealed partial class GameControllerSingleton
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