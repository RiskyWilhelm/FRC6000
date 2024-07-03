using System;
using UnityEngine.Events;

public sealed partial class ChickenFoxExtinctionControllerSingleton : ExtinctionControllerSingletonBase<ChickenFoxExtinctionControllerSingleton>
{
	#region ChickenFoxExtinctionControllerSingleton Rate Verify

	public int ChickenRate { get; private set; }

	public int FoxRate { get; private set; }

	public override int CurrentRate
	{
		get => base.CurrentRate;
		protected set
		{
			var newValue = (int)Math.Clamp(value, -MaxRate * 0.5f, MaxRate * 0.5f);

			if (_currentRate != newValue)
			{
				_currentRate = newValue;
				OnCurrentRateChanged(newValue);
			}
		}
	}


	#endregion

	#region ChickenFoxExtinctionControllerSingleton Events

	public UnityEvent onChickenExtinctRateFullFilled = new();

	public UnityEvent onFoxExtinctRateFullFilled = new();


	#endregion


	// Initialize
	private void OnEnable()
	{
		// OPTIMIZATION: You know, passing methods as delegate are creating garbage every time... Instead, pass this (() => DecreaseRate());
		PlayerControllerSingleton.Instance.onTargetBirthEventDict[TargetType.BabyChicken] += DecreaseRate;
		PlayerControllerSingleton.Instance.onTargetDeathEventDict[TargetType.BabyChicken] += IncreaseRate;

		PlayerControllerSingleton.Instance.onTargetDeathEventDict[TargetType.ChickenHome] += IncreaseRate;

		PlayerControllerSingleton.Instance.onTargetDeathEventDict[TargetType.WarrirorChicken] += IncreaseRate;

		PlayerControllerSingleton.Instance.onTargetBirthEventDict[TargetType.BabyFox] += IncreaseRate;
		PlayerControllerSingleton.Instance.onTargetDeathEventDict[TargetType.BabyFox] += DecreaseRate;

		PlayerControllerSingleton.Instance.onTargetDeathEventDict[TargetType.WarrirorFox] += DecreaseRate;
	}


	// Update
	private void UpdateRatesByCurrentRate()
	{
		ChickenRate = (_currentRate < 0) ? Math.Abs(_currentRate) : 0;
		FoxRate = (_currentRate > 0) ? Math.Abs(_currentRate) : 0;
	}

	private void CheckRates()
	{
		var extinctFullFilled = (MaxRate * 0.5f);

		if (ChickenRate == extinctFullFilled)
			OnChickenRateFullFilled();
		else if (FoxRate == extinctFullFilled)
			OnFoxRateFullFilled();
	}

	protected override void OnCurrentRateChanged(int newValue)
	{
		UpdateRatesByCurrentRate();
		CheckRates();
		base.OnCurrentRateChanged(newValue);
	}

	private void OnChickenRateFullFilled()
		=> onChickenExtinctRateFullFilled?.Invoke();

	private void OnFoxRateFullFilled()
		=> onFoxExtinctRateFullFilled?.Invoke();


	// Dispose
	private void OnDisable()
	{
		if (GameControllerSingleton.IsQuitting)
			return;

		PlayerControllerSingleton.Instance.onTargetBirthEventDict[TargetType.BabyChicken] -= DecreaseRate;
		PlayerControllerSingleton.Instance.onTargetDeathEventDict[TargetType.BabyChicken] -= IncreaseRate;

		PlayerControllerSingleton.Instance.onTargetDeathEventDict[TargetType.ChickenHome] -= IncreaseRate;

		PlayerControllerSingleton.Instance.onTargetDeathEventDict[TargetType.WarrirorChicken] -= IncreaseRate;

		PlayerControllerSingleton.Instance.onTargetBirthEventDict[TargetType.BabyFox] -= IncreaseRate;
		PlayerControllerSingleton.Instance.onTargetDeathEventDict[TargetType.BabyFox] -= DecreaseRate;

		PlayerControllerSingleton.Instance.onTargetDeathEventDict[TargetType.WarrirorFox] -= DecreaseRate;
	}
}


#if UNITY_EDITOR

public sealed partial class ChickenFoxExtinctionControllerSingleton
{ }

#endif