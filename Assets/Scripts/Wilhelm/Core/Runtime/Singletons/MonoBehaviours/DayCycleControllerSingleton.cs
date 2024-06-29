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

	public float daySpeed;

	public Color dayLightColor;

	public Color dayNightColor;


	#endregion

	[Header("DayCycleControllerSingleton Time")]
	#region Time

	private GameTime _time;

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

			if (value.isDayChangeHour != _time.isDayChangeHour)
				onDayChanged?.Invoke();

			_time = value;
		}
	}


	#endregion

	[Header("DayCycleControllerSingleton Events")]
	#region DayCycleControllerSingleton Events

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

	private void InitializeTime()
	{
		_time = GameTimeUtils.AngleToGameTime(this.transform.rotation.eulerAngles.z);
		isTimeInitialized = true;
	}

	/// <summary> Assumes the time 12:00 equals North(Y+) in cardinal direction </summary>
	/*private float GameTimeToAngle(GameTime gameTime, sbyte offsetHour = 0)
	{
		// offsetAngleDegree is used to shift the right side (angle zero) time to something else instead of 00:00
		float exactHourPoint = (360 / 24);
		float exactMinutePoint = (exactHourPoint / 60f);
		float exactSecondPoint = (exactMinutePoint / 60f);

		float offsetAngleDegree = 270;
		float offsetHourAngle = (offsetHour * exactHourPoint) + offsetAngleDegree;

		float angleInDegrees = (gameTime.hour * exactHourPoint);
		float verticallyMirroredAngleDegree = (360 - angleInDegrees); // Not inverted! Mirrored the angle in a circle. That way, time will grow if rotation getting negative

		float hourAngle = verticallyMirroredAngleDegree + offsetHourAngle;
		float minuteAngle = (gameTime.minute * exactMinutePoint);
		float secondAngle = (gameTime.second * exactSecondPoint);

		return Math.Abs(hourAngle - minuteAngle - secondAngle) % 360;
	}*/

	/// <summary> Assumes the time 12:00 equals North(Y+) in cardinal direction </summary>
	/*private float GameTimeToAngle(GameTime gameTime, sbyte offsetHour = 0)
	{
		// offsetAngleDegree is used to shift the right side (angle zero) time to something else instead of 00:00
		float exactHourPoint = (360 / 24);
		float exactMinutePoint = (exactHourPoint / 60f);
		float exactSecondPoint = (exactMinutePoint / 60f);
		float offsetAngleDegree = 270;

		float hourAngle = (360 - (gameTime.hour * exactHourPoint)) + offsetAngleDegree;
		float minuteAngle = (gameTime.minute * exactMinutePoint);
		float secondAngle = (gameTime.second * exactSecondPoint);

		return Math.Abs(hourAngle - minuteAngle - secondAngle) % 360;
	}*/
}


#if UNITY_EDITOR

public sealed partial class DayCycleControllerSingleton
{
	public string e_Time;
	public float e_TimeAngle;
	public float e_TimeProgress;
	public string e_Time12;

	private void LateUpdate()
	{
		e_Time = _time.ToString();
		e_Time12 = _time.To12HourTime().ToString();
		e_TimeAngle = _time.ToAngle();
		e_TimeProgress = _time.ToProgress01();
	}
}

#endif