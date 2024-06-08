using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

// TODO: This class will need a rework after if game gets popular. Because this is not very extendable
public sealed partial class DayCycleControllerSingleton : MonoBehaviourSingletonBase<DayCycleControllerSingleton>
{
	[Header("DayCycleControllerSingleton Movement")]
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

	public UnityEvent<DaylightType> onDaylightTypeChanged = new();

	public UnityEvent onDayChanged = new();

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

			if (value.isDayChangeHour != _time.isDayChangeHour)
				onDayChanged?.Invoke();

			_time = value;
		}
	}

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

		// By default, unity will rotate to LEFT when z rotation of a Transform is increased. We dont want that because the sun should rise from left side
		// Mirror the circular rotation by subtracting with 360. Ex. (350 - 360) = -10 which means it grows to the other side instead of the current side
		Time = GetCurrentTimeInAngle(this.transform.rotation.eulerAngles.z - 360, 12);
	}

	private void UpdateSun()
	{
		// Rotate the sun. Negated the dayspeed because the sun never rise from right and negative means "rotate to right"
		this.transform.Rotate(0, 0, -daySpeed * UnityEngine.Time.deltaTime);

		// Change color based on rotation
		var currentZAngle = this.transform.rotation.eulerAngles.z;
		var exactLightPoint = 180;
		var lightAnglePointWithProcess = Mathf.Abs((currentZAngle - (exactLightPoint * Mathf.Sign(currentZAngle))) / exactLightPoint);
		sun.color = Color.Lerp(dayNightColor, dayLightColor, lightAnglePointWithProcess);
	}

	private void InitializeTime()
	{
		_time = GetCurrentTimeInAngle(this.transform.rotation.eulerAngles.z - 360, 12);
		isTimeInitialized = true;
	}

	// TODO: Refactor the code
	/// <param name="offsetHour"> Offsets the angle by (exact hour points * offsetHour). Ex: when (angle = {0}, offsetHour = {1 or -1}) result.hour = {1} </param>
	private GameTime GetCurrentTimeInAngle(float angle, byte offsetHour = 0)
	{
		// Snap the angle to 0-360 degree(negative allowed) by (value % 360) and get hour process
		float exactHourPoint = (360/24);

		float offsetAngle = offsetHour * exactHourPoint * Mathf.Sign(angle) * Mathf.Sign(offsetHour);
		float hourWithProcess = Mathf.Abs( ((angle + offsetAngle) % 360) / exactHourPoint);

		// Get how long it will take to the next hours in a range of 0-1
		float processToNextHour = (hourWithProcess + 1f) - Mathf.Ceil(hourWithProcess);
		float minuteWithProcess = processToNextHour * 60f;

		float processToNextMin = (minuteWithProcess + 1f) - Mathf.Ceil(minuteWithProcess);
		float secondWithProcess = processToNextMin * 60f;

		return new GameTime((byte)hourWithProcess, (byte)minuteWithProcess, (byte)secondWithProcess);
	}
}


#if UNITY_EDITOR

public sealed partial class DayCycleControllerSingleton
{
	public string e_Time;
	public string e_Time12;

	private void LateUpdate()
	{
		e_Time = _time.ToString();
		e_Time12 = new GameTime(_time.hour, _time.minute, _time.second, convertToPM: true).ToString();
	}
}

#endif