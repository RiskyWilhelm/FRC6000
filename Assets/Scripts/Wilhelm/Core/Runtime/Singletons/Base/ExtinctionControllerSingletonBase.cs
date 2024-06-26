using System;
using UnityEngine;

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

	[field: Header("ExtinctionControllerSingletonBase Rate Verify")]
	#region ExtinctionControllerSingletonBase Rate Verify

	[SerializeField]
	[ContextMenuItem(nameof(IncreaseRate), nameof(IncreaseRate))]
	[ContextMenuItem(nameof(DecreaseRate), nameof(DecreaseRate))]
	[ContextMenuItem(nameof(MoveVisualToCurrentRate), nameof(MoveVisualToCurrentRate))]
	protected int _currentRate;

	[field: SerializeField]
	public ushort MaxRate { get; protected set; }

	// TODO: Ridicilous. Refactor in next version
	public virtual int CurrentRate
	{
		get => _currentRate;
		protected set
		{
			var newValue = Math.Clamp(value, 0, MaxRate);

			if (_currentRate != newValue)
			{
				_currentRate = newValue;
				OnCurrentRateChanged(newValue);
			}
		}
	}


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
		// Get horizontal step position for every x rate dependent on Max Rect Transform width to show correctly
		var rateScreenStepHPosition = maxRTransform.rect.width;

		// Handle division by zero
		if (MaxRate != 0)
			rateScreenStepHPosition = (maxRTransform.rect.width / MaxRate);

		// Show the current rate
		currentRateRTransform.anchoredPosition = new Vector2(
			rateScreenStepHPosition * CurrentRate,
			currentRateRTransform.anchoredPosition.y);
	}

	public void IncreaseRate()
		=> CurrentRate++;

	public void DecreaseRate()
		=> CurrentRate--;

	protected virtual void OnCurrentRateChanged(int newValue)
		=> MoveVisualToCurrentRate();
}


#if UNITY_EDITOR

public abstract partial class ExtinctionControllerSingletonBase<SingletonType>
{ }

#endif