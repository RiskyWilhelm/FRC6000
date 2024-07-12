using UnityEngine;
using UnityEngine.Events;

public sealed partial class ActivatedByDayConfiguration : MonoBehaviour
{
	[Header("ActivatedByDayConfiguration Activation")]
	#region ActivatedByDayConfiguration Activation

	[SerializeField]
	private DayType dayType;

	[SerializeField]
	private DaylightType daylightType;


	#endregion

	[Header("ActivatedByDayConfiguration Events")]
	#region ActivatedByDayConfiguration Events

	[SerializeField]
	private UnityEvent onActivated = new();

	[SerializeField]
	private UnityEvent onDeactivated = new();


	#endregion


	// Initialize
	private void Start()
	{
		DayCycleControllerSingleton.Instance.onDayTypeChanged.AddListener(OnDayTypeChanged);
		DayCycleControllerSingleton.Instance.onDaylightTypeChanged.AddListener(OnDaylightTypeChanged);
		OnDayTypeChanged(DayCycleControllerSingleton.Instance.GameTimeDayType);
		OnDaylightTypeChanged(DayCycleControllerSingleton.Instance.GameTimeDaylightType);
	}


	// Update
	private void OnDaylightTypeChanged(DaylightType newDaylightType)
	{
		if (daylightType is DaylightType.None)
			return;

		if (daylightType.HasFlag(newDaylightType))
			onActivated?.Invoke();
		else
			onDeactivated?.Invoke();
	}

	private void OnDayTypeChanged(DayType newDayType)
	{
		if (dayType is DayType.None)
			return;

		if (dayType.HasFlag(newDayType))
			onActivated?.Invoke();
		else
			onDeactivated?.Invoke();
	}


	// Dispose
	private void OnDestroy()
	{
		DayCycleControllerSingleton.Instance.onDayTypeChanged.RemoveListener(OnDayTypeChanged);
		DayCycleControllerSingleton.Instance.onDaylightTypeChanged.RemoveListener(OnDaylightTypeChanged);
	}
}


#if UNITY_EDITOR

public sealed partial class ActivatedByDayConfiguration
{ }

#endif