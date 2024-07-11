using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

// TODO: This class will need a rework after if game gets popular. Because this is not very extendable
public sealed partial class DayCycleControllerSingleton : MonoBehaviourSingletonBase<DayCycleControllerSingleton>
{
	[Header("DayCycleControllerSingleton Sun")]
	#region DayCycleControllerSingleton Sun

	[SerializeField]
	private Light2D sun;


	#endregion

	[Header("DayCycleControllerSingleton Game Day")]
	#region DayCycleControllerSingleton Day

	[SerializeField]
	private float daySpeed;

	[SerializeField]
	private Color dayLightColor;

	[SerializeField]
	private Color dayNightColor;


	#endregion

	[Header("DayCycleControllerSingleton Game Time")]
	#region DayCycleControllerSingleton Game Time

	[NonSerialized]
	private DateTime _gameTime;

	[NonSerialized]
	private DaylightType _gameTimeDaylightType;

	[NonSerialized]
	private DayType _gameTimeDayType;

	[NonSerialized]
	private bool isGameTimeInitialized;

	public DaylightType GameTimeDaylightType
	{
		get
		{
			if (!isGameTimeInitialized)
				InitializeTime();

			return _gameTimeDaylightType;
		}

		set
		{
			if (value == _gameTimeDaylightType)
				return;

			_gameTimeDaylightType = value;
			onDaylightTypeChanged?.Invoke(value);
		}
	}

	public DayType GameTimeDayType
	{
		get
		{
			if (!isGameTimeInitialized)
				InitializeTime();

			return _gameTimeDayType;
		}

		set
		{
			if (value == _gameTimeDayType)
				return;

			_gameTimeDayType = value;
			onDayTypeChanged?.Invoke(value);
		}
	}

	public DateTime GameTime
	{
		get
		{
			if (!isGameTimeInitialized)
				InitializeTime();

			return _gameTime;
		}

		set
		{
			if (IsDaylight(value))
				GameTimeDaylightType = DaylightType.Light;
			else
				GameTimeDaylightType = DaylightType.Night;

			if (value.Hour >= 12)
				GameTimeDayType = DayType.PM;
			else
				GameTimeDayType = DayType.AM;

			if ((_gameTimeDayType is DayType.AM) && (value.Hour == 0) && (_gameTime.Hour != 0))
				onDayChanged?.Invoke();

			_gameTime = value;
			onGameTimeChanged?.Invoke(value, "HH:mm:ss tt");
			this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, value.ToAngleDegree()));
		}
	}


	#endregion

	[Header("DayCycleControllerSingleton Events")]
	#region DayCycleControllerSingleton Events

	public UnityEvent<DateTime, string> onGameTimeChanged = new();

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
		if (!isGameTimeInitialized)
		{
			GameTime = DateTimeUtils.AngleDegreeToDateTime(this.transform.rotation.eulerAngles.z);
			isGameTimeInitialized = true;
		}
	}


	// Update
	private void Update()
	{
		GameTime = _gameTime.AddSeconds(daySpeed * Time.deltaTime);
		UpdateSun();
	}

	private void UpdateSun()
	{
		// Change color based on time
		var infiniteDayProgress = MathF.Abs(MathF.Cos(_gameTime.ToProgress01() * MathF.PI));
		sun.color = Color.Lerp(dayLightColor, dayNightColor, infiniteDayProgress);
	}

	public bool IsDaylight(DateTime a)
	{
		return (a.Hour >= 6) && (a.Hour <= 19);
	}

	public bool IsDaylight()
		=> IsDaylight(_gameTime);
}


#if UNITY_EDITOR

public sealed partial class DayCycleControllerSingleton
{ }

#endif