using UnityEngine;
using UnityEngine.SceneManagement;

public sealed partial class SceneControllerPersistentSingleton : MonoBehaviourSingletonBase<SceneControllerPersistentSingleton>
{
	public static bool IsActiveSceneChanging { get; private set; }


	// Update
	public void RestartScene(bool unloadUnusedAssets = false)
	{
		if (unloadUnusedAssets)
		{
			Resources.UnloadUnusedAssets().completed += RestartSceneWithoutUnusedAssets;
			return;
		}

		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	private void RestartSceneWithoutUnusedAssets(AsyncOperation operation)
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	/// <summary>
	/// <see cref="RuntimeInitializeOnLoadMethodAttribute"/> does not respect the order of execution. <see cref="SceneControllerPersistentSingleton"/> needed a parent and this is <see cref="GameControllerPersistentSingleton"/>
	/// <br/>
	/// By doing that, <see cref="SceneControllerPersistentSingleton"/> will respect the parent's variables
	/// </summary>
	public static void OnGameControllerActiveSceneChanged(Scene lastScene, Scene loadedScene)
	{
		IsActiveSceneChanging = false;

		if (!IsAnyInstanceLiving)
			TryCreateSingleton();
	}


	// Dispose
	private void OnDestroy()
	{
		IsActiveSceneChanging = true;
	}
}


#if UNITY_EDITOR

public sealed partial class SceneControllerPersistentSingleton
{ }

#endif