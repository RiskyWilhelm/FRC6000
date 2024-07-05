using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

// TODO: This class will need a rework after if game gets popular. Because this is not very extendable
public sealed partial class DayCycleControllerSingleton : MonoBehaviourSingletonBase<DayCycleControllerSingleton>
{
	[Header("DayCycleControllerSingleton Sun")]
	#region

	[SerializeField]
	private Light2D sun;

	[SerializeField]
	private float daySpeed;

	[SerializeField]
	private Color dayLightColor;

	[SerializeField]
	private Color dayNightColor;


	#endregion

	[Header("DayCycleControllerSingleton Time")]
	#region Time

	[NonSerialized]
	private GameTime _time;

	[NonSerialized]
	private bool isTimeInitialized;

	public GameTime Time
	{
		get
		{
			if (!isTimeInitialized)
				InitializeTime();

			return _time;
		}

		private set
		{
			if (value.daylightType != _time.daylightType)
				onDaylightTypeChanged?.Invoke(value.daylightType);

			if (value.dayType != _time.dayType)
				onDayTypeChanged?.Invoke(value.dayType);

			if ((value.dayType is DayType.AM) && (value.hour == 0) && (_time.hour != 0))
				onDayChanged?.Invoke();

			onGameTimeChanged?.Invoke(value);
			_time = value;
		}
	}


	#endregion

	[Header("DayCycleControllerSingleton Events")]
	#region DayCycleControllerSingleton Events

	public UnityEvent<GameTime> onGameTimeChanged = new();

	public UnityEvent<DaylightType> onDaylightTypeChanged = new();

	public UnityEvent<DayType> onDayTypeChanged = new();

	public UnityEvent onDayChanged = new();


	#endregion


	// Initialize
	protected override void Awake()
	{
		InitializeTime();
		base.Awake();
	}

	private void InitializeTime()
	{
		if (!isTimeInitialized)
		{
			_time = GameTimeUtils.AngleToGameTime(this.transform.rotation.eulerAngles.z);
			onDaylightTypeChanged?.Invoke(_time.daylightType);
			onDayTypeChanged?.Invoke(_time.dayType);
			isTimeInitialized = true;
		}
	}


	// Update
	private void Update()
	{
		UpdateSun();
		Time = GameTimeUtils.AngleToGameTime(this.transform.rotation.eulerAngles.z);
	}

	private void UpdateSun()
	{
		// Rotate sun
		this.transform.Rotate(0, 0, -daySpeed * UnityEngine.Time.deltaTime);

		// Change color based on rotation
		var infiniteDayProgress = MathF.Abs(MathF.Cos(_time.ToProgress01() * MathF.PI));
		sun.color = Color.Lerp(dayLightColor, dayNightColor, infiniteDayProgress);
	}
}


#if UNITY_EDITOR

public sealed partial class DayCycleControllerSingleton
{
	public string e_Time;
	public string e_Time12;

	private void LateUpdate()
	{
		/*e_Time = _time.ToString();
		e_Time12 = _time.To12HourTime().ToString();*/
	}
}

#endif