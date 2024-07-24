using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public sealed partial class PlayerControllerSingleton : MonoBehaviourSingletonBase<PlayerControllerSingleton>
{
	#region PlayerControllerSingleton Spawn

	[SerializeField]
	private Vector2 playerSpawnWorldPosition;

	[SerializeField]
	private Player[] differentPlayerObjects = new Player[0];

	private static readonly System.Random spawnerRandom = new ();


	#endregion

	#region PlayerControllerSingleton Events

	private static readonly Dictionary<TargetType, Action> _onTargetBirthEventDict = new();

	private static readonly Dictionary<TargetType, Action> _onTargetDeathEventDict = new();

	public static Dictionary<TargetType, Action> onTargetBirthEventDict
	{
		get
		{
			if (_onTargetBirthEventDict.Count == 0)
			{
				foreach (TargetType iteratedTargetType in Enum.GetValues(typeof(TargetType)))
					_onTargetBirthEventDict.TryAdd(iteratedTargetType, null);
			}

			return _onTargetBirthEventDict;
		}
	}

	public static Dictionary<TargetType, Action> onTargetDeathEventDict
	{
		get
		{
			if (_onTargetDeathEventDict.Count == 0)
			{
				foreach (TargetType iteratedTargetType in Enum.GetValues(typeof(TargetType)))
					_onTargetDeathEventDict.TryAdd(iteratedTargetType, null);
			}

			return _onTargetDeathEventDict;
		}
	}


	#endregion


	// Initialize
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void OnBeforeSplashScreen()
	{
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
	}

	private IEnumerator Start()
	{
		yield return null;

		if (differentPlayerObjects.Length != 0)
		{
			var randomPlayerObject = differentPlayerObjects[spawnerRandom.Next(differentPlayerObjects.Length)];
			Instantiate(randomPlayerObject, playerSpawnWorldPosition, Quaternion.identity);
		}
	}


	// Update
	private static void OnActiveSceneChanged(Scene lastScene, Scene loadedScene)
	{
		if (!IsAnyInstanceLiving)
			TryCreateSingleton();
	}
}


#if UNITY_EDITOR

public sealed partial class PlayerControllerSingleton
{
	private void OnDrawGizmosSelected()
	{
		DrawPlayerSpawnWorldPosition();
	}

	private void DrawPlayerSpawnWorldPosition()
	{
		Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
		Gizmos.DrawSphere(playerSpawnWorldPosition, 1f);
	}
}

#endif