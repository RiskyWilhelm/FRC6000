using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class PlayerControllerSingleton : MonoBehaviourSingletonBase<PlayerControllerSingleton>
{
	#region PlayerControllerSingleton Events

	public readonly Dictionary<TargetType, Action> onTargetBirthEventDict = new();

	public readonly Dictionary<TargetType, Action> onTargetDeathEventDict = new();


	#endregion


	// Initialize
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnRuntimeInitialization()
	{
		if (!IsInstanceLiving)
			FindOrCreate();

		foreach (TargetType iteratedTargetType in Enum.GetValues(typeof(TargetType)))
		{
			Instance.onTargetBirthEventDict.Add(iteratedTargetType, null);
			Instance.onTargetDeathEventDict.Add(iteratedTargetType, null);
		}

		Debug.LogFormat("Initialized PlayerController '{0}'", Instance.GameObjectName);
	}

	protected override void Awake()
	{
		DontDestroyOnLoad(Instance);
		base.Awake();
	}
}


#if UNITY_EDITOR

public sealed partial class PlayerControllerSingleton
{ }

#endif