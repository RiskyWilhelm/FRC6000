using UnityEngine;
using UnityEngine.SceneManagement;

public sealed partial class SceneControllerPersistentSingleton : MonoBehaviourSingletonBase<SceneControllerPersistentSingleton>
{
	public static bool IsActiveSceneChanging { get; private set; }

	// Initialize
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void OnBeforeSplashScreen()
	{
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
	}


	// Update
	public void ChangeActiveScene(string sceneName)
	{
		IsActiveSceneChanging = true;
		SceneManager.LoadScene(sceneName);
	}

	public void RestartScene(bool unloadUnusedAssets = false)
	{
		if (unloadUnusedAssets)
		{
			Resources.UnloadUnusedAssets().completed += RestartSceneWithoutUnusedAssets;
			return;
		}

		ChangeActiveScene(SceneManager.GetActiveScene().name);
	}

	private void RestartSceneWithoutUnusedAssets(AsyncOperation operation)
	{
		ChangeActiveScene(SceneManager.GetActiveScene().name);
	}

	private static void OnActiveSceneChanged(Scene lastScene, Scene loadedScene)
	{
		IsActiveSceneChanging = false;

		if (!IsAnyInstanceLiving)
			TryCreateSingleton();
	}
}


#if UNITY_EDITOR

public sealed partial class SceneControllerPersistentSingleton
{ }

#endif