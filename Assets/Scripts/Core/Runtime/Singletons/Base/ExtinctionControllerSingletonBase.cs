using System;
using UnityEngine;
using UnityEngine.Events;

public abstract partial class ExtinctionControllerSingletonBase<SingletonType> : MonoBehaviourSingletonBase<SingletonType>
	where SingletonType : ExtinctionControllerSingletonBase<SingletonType>
{
	[Header("ExtinctionControllerSingletonBase Visuals")]
	#region ExtinctionControllerSingletonBase Visuals

	[SerializeField]
	protected RectTransform maxRTransform;

	[SerializeField]
	protected RectTransform currentRateRTransform;


	#endregion

	[field: Header("ExtinctionControllerSingletonBase Rate")]
	#region ExtinctionControllerSingletonBase Rate

	[SerializeField]
	[ContextMenuItem(nameof(IncreaseRate), nameof(IncreaseRate))]
	[ContextMenuItem(nameof(DecreaseRate), nameof(DecreaseRate))]
	[ContextMenuItem(nameof(MoveVisualToCurrentRate), nameof(MoveVisualToCurrentRate))]
	protected int _currentRate;

	[SerializeField]
	protected int minRate;

	[SerializeField]
	protected int maxRate;

	public int CurrentRate
	{
		get => _currentRate;
		protected set
		{
			var newValue = Math.Clamp(value, minRate, maxRate);

			if (_currentRate != newValue)
			{
				_currentRate = newValue;
				OnCurrentRateChanged(newValue);
			}
		}
	}


	#endregion

	[Header("ExtinctionControllerSingletonBase Events")]
	#region ExtinctionControllerSingletonBase Events

	public UnityEvent onHitMinRate = new();

	public UnityEvent onHitMaxRate = new();


	#endregion


	// Initialize
	protected virtual void Start()
	{
		MoveVisualToCurrentRate();
	}


	// Update
	/// <summary> Does a horizontal movement </summary>
	protected virtual void MoveVisualToCurrentRate()
	{
		// Get horizontal step position for each rate change
		var rateScreenStepHorizontally = Math.Abs(minRate) + Math.Abs(maxRate);
		var rateScreenStepHPosition = checked(maxRTransform.rect.width / rateScreenStepHorizontally);

		// Show the current rate
		currentRateRTransform.anchoredPosition = new Vector2(
			rateScreenStepHPosition * CurrentRate,
			currentRateRTransform.anchoredPosition.y);
	}

	public void IncreaseRate()
		=> CurrentRate++;

	public void DecreaseRate()
		=> CurrentRate--;

	private void CheckRates()
	{
		if (CurrentRate == minRate)
			onHitMinRate?.Invoke();
		else if (CurrentRate == maxRate)
			onHitMaxRate?.Invoke();
	}

	protected virtual void OnCurrentRateChanged(int newValue)
	{
		MoveVisualToCurrentRate();
		CheckRates();
	}
}


#if UNITY_EDITOR

public abstract partial class ExtinctionControllerSingletonBase<SingletonType>
{
	public void E_FullfillRate()
	{
		CurrentRate = maxRate;
	}
}

#endif