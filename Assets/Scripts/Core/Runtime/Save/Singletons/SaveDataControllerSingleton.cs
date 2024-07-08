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

	private static SaveData _data;

	public static SaveData Data
	{
		get
		{
			if (_data == null)
			{
				Instance.LoadDataFromFile();
				_data ??= new SaveData();
			}

            return _data;
		}
	}

	public static string FullSavePath => Path.Combine(Application.persistentDataPath, IOUtils.FixPathByCorrectDirectorySeperator("FRCMainSave.json"));


	#endregion


	// Update
	public void LoadDataFromFile()
    {
        if (IOUtils.Load<SaveData>(FullSavePath, out _data))
			onLoad?.Invoke(_data);
    }

    public void SaveDataToFile()
    {
        IOUtils.Save<SaveData>(_data, FullSavePath);
		onSave?.Invoke(_data);
    }

	public void DeleteSaveDataFile()
	{
		IOUtils.Delete(FullSavePath);
	}

	public void FreshSaveData()
	{
		_data = new();
		onLoad?.Invoke(_data);
	}
}


#if UNITY_EDITOR

public sealed partial class SaveDataControllerSingleton
{ }

#endif