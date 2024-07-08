public sealed partial class ChickenFoxExtinctionControllerSingleton : ExtinctionControllerSingletonBase<ChickenFoxExtinctionControllerSingleton>, ILoadableSaveData
{
	// Initialize
	private void OnEnable()
	{
		// OPTIMIZATION: You know, passing methods as delegate are creating garbage every time... Instead, pass this (() => DecreaseRate());
		PlayerControllerSingleton.onTargetBirthEventDict[TargetType.BabyChicken] += DecreaseRate;
		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.BabyChicken] += IncreaseRate;

		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.ChickenHome] += IncreaseRate;

		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.WarrirorChicken] += IncreaseRate;

		PlayerControllerSingleton.onTargetBirthEventDict[TargetType.BabyFox] += IncreaseRate;
		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.BabyFox] += DecreaseRate;

		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.WarrirorFox] += DecreaseRate;

		LoadSaveData(SaveDataControllerSingleton.Data);
	}


	// Update
	public void LoadSaveData(SaveData saveData)
	{
		CurrentRate = saveData.chickenFoxExtinctionRate;
	}

	public void OverrideSaveData()
	{
		SaveDataControllerSingleton.Data.chickenFoxExtinctionRate = CurrentRate;
	}

	protected override void OnCurrentRateChanged(int newValue)
	{
		OverrideSaveData();
		base.OnCurrentRateChanged(newValue);
	}


	// Dispose
	private void OnDisable()
	{
		PlayerControllerSingleton.onTargetBirthEventDict[TargetType.BabyChicken] -= DecreaseRate;
		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.BabyChicken] -= IncreaseRate;

		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.ChickenHome] -= IncreaseRate;

		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.WarrirorChicken] -= IncreaseRate;

		PlayerControllerSingleton.onTargetBirthEventDict[TargetType.BabyFox] -= IncreaseRate;
		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.BabyFox] -= DecreaseRate;

		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.WarrirorFox] -= DecreaseRate;
	}
}


#if UNITY_EDITOR

public sealed partial class ChickenFoxExtinctionControllerSingleton
{ }

#endif