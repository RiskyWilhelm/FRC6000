using UnityEngine;
using UnityEngine.Rendering.Universal;

public sealed partial class DayCycleLight : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField]
	private Light2D sun;

	public float daySpeed;

	public Color dayLightColor;

	public Color dayNightColor;


	// Time
	public GameTime Time { get; private set; }


	// Update
	private void Update()
	{
		UpdateSunRotation();
		UpdateSunColorByTime();
		UpdateTime();
	}

	private void UpdateSunRotation()
	{
		// Rotate the sun. Negated the dayspeed because the sun never rise from right and negative means "rotate to right"
		this.transform.Rotate(0, 0, -daySpeed * UnityEngine.Time.deltaTime);
	}

	private void UpdateTime()
	{
		// By default, unity will rotate to LEFT when z rotation of a Transform is increased. We dont want that because the sun should rise from left side
		// Mirror the circular rotation by subtracting with 360. Ex. (350 - 360) = -10 which means it grows to the other side instead of the current side
		Time = GetCurrentTimeInAngle(this.transform.rotation.eulerAngles.z - 360, 12);
	}

	private void UpdateSunColorByTime()
	{
		var currentZAngle = this.transform.rotation.eulerAngles.z;
		var exactLightPoint = 180;
		var lightAnglePointWithProcess = Mathf.Abs((currentZAngle - (exactLightPoint * Mathf.Sign(currentZAngle))) / exactLightPoint);
		sun.color = Color.Lerp(dayNightColor, dayLightColor, lightAnglePointWithProcess);
	}

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

public sealed partial class DayCycleLight
{
	public string e_Time;

	private void LateUpdate()
	{
		e_Time = GetCurrentTimeInAngle(this.transform.rotation.eulerAngles.z - 360, 12).ToString();
	}
}

#endif