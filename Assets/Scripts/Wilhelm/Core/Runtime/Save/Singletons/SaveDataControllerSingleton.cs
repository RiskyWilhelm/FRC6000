using System.IO;
using UnityEngine;
using UnityEngine.Events;

public sealed partial class SaveDataControllerSingleton : MonoBehaviourSingletonBase<SaveDataControllerSingleton>
{
	[Header("SaveDataControllerSingleton Events")]
	#region SaveDataControllerSingleton Events

	public UnityEvent<SaveData> onSave = new();

	public UnityEvent<SaveData> onLoad = new();


	#endregion

	#region SaveDataControllerSingleton Save

	public SaveData Save { get; private set; } = new();

	public static string FullSavePath => Path.Combine(Application.persistentDataPath, IOUtils.FixPathByCorrectDirectorySeperator("FRCMainSave.json"));


	#endregion


	// Initialize
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoad()
	{
		if (!IsInstanceLiving)
			FindOrCreate();

        Instance.LoadDataFromFile();
		Debug.LogFormat("Initialized '{0}'", Instance.GameObjectName);
	}


	// Update
	public void LoadDataFromFile()
    {
        if (IOUtils.Load<SaveData>(FullSavePath, out SaveData loadedData))
		{
            Save = loadedData;
			onLoad?.Invoke(Save);
		}
    }

    public void SaveDataToFile()
    {
        IOUtils.Save<SaveData>(Save, FullSavePath);
		onSave?.Invoke(Save);
    }
}


#if UNITY_EDITOR

public sealed partial class SaveDataControllerSingleton
{ }

#endif