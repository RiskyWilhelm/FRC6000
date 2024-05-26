using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public sealed partial class DayCycleLight : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField]
	private Light2D sun;

	public float daySpeed;

	public Color dayLightColor;

	public Color dayNightColor;

	private Vector2 previousTransformRight;

	private float totalRotatedAngle;

	public UnityEvent onCompletedFullTurn;

	// Time
	[field: SerializeField]
	public string Time { get; private set; }

	private static readonly StringBuilder timeBuilder = new();


	// Initialize
	private void Start()
	{
		previousTransformRight = this.transform.right;
	}


	// Update
	private void Update()
	{
		UpdateSun();
		UpdateSunColorByTime();
		UpdateTime();
	}

	private void UpdateSun()
	{
		// Rotate the sun. Negated the dayspeed because the sun never rise from right and negative means "rotate to right"
		this.transform.Rotate(0, 0, -daySpeed * UnityEngine.Time.deltaTime);

		// Check if it turned fully by comparing it to the previous frame's right vector and summing up the delta angle
		var currentRight = this.transform.right;
		totalRotatedAngle += Vector2.SignedAngle(previousTransformRight, currentRight);

		// did the angle reach +/- 360 ? Is Completed full turn?
		if (Mathf.Abs(totalRotatedAngle) >= 360f)
		{
			Debug.Log("Completed full turn");
			onCompletedFullTurn?.Invoke();

			// if _angle > 360 subtract 360
			// if _angle < -360 add 360
			totalRotatedAngle -= 360f * Mathf.Sign(totalRotatedAngle);
		}

		previousTransformRight = currentRight;
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

	public string GetCurrentTimeInAngle(float angle, byte offsetHour = 0)
	{
		timeBuilder.Clear();

		// Snap the angle to 0-360 degree(negative allowed) by (value % 360) and get hour process
		float exactHourPoint = (360/24);

		float offsetAngle = offsetHour * exactHourPoint * Mathf.Sign(angle) * Mathf.Sign(offsetHour);
		float hourWithProcess = Mathf.Abs( ((angle + offsetAngle) % 360) / exactHourPoint);

		// Get how long it will take to the next hours in a range of 0-1
		float processToNextHour = (hourWithProcess + 1f) - Mathf.Ceil(hourWithProcess);
		float minuteWithProcess = processToNextHour * 60f;

		float processToNextMin = (minuteWithProcess + 1f) - Mathf.Ceil(minuteWithProcess);
		float secondWithProcess = processToNextMin * 60f;

		timeBuilder.AppendFormat("{0:00}:{1:00}:{2:00}", (int)hourWithProcess, (int)minuteWithProcess, (int)secondWithProcess);
		return timeBuilder.ToString();
	}
}


#if UNITY_EDITOR

public sealed partial class DayCycleLight
{ }

#endif