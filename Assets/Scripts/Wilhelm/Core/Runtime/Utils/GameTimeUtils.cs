using System;

public static class GameTimeUtils
{
	/// <summary> Assumes the time 12:00 equals North(Y+ when facing to Z+) in cardinal direction </summary>
	public static GameTime AngleToGameTime(float angleInDegrees, sbyte offsetHour = 0, bool convertTo12HourTime = false)
	{
		float exactHourPoint = (360 / 24);

		// offsetAngleDegree is used to shift the right side (angle zero) time to something else instead of 00:00
		float verticallyMirroredAngleDegree = (360 - angleInDegrees); // Not inverted! Mirrored the angle in a circle. That way, time will grow if rotation getting negative
		float offsetAngleDegree = 270;

		float offsetHourAngle = (offsetHour * exactHourPoint) + offsetAngleDegree;
		float hourWithProcess = (MathF.Abs(verticallyMirroredAngleDegree + offsetHourAngle) % 360) / exactHourPoint;

		// Get how long it will take to the next hours in a range of 0-1
		float processToNextHour = (hourWithProcess + 1f) - MathF.Ceiling(hourWithProcess);
		float minuteWithProcess = processToNextHour * 60f;

		float processToNextMin = (minuteWithProcess + 1f) - MathF.Ceiling(minuteWithProcess);
		float secondWithProcess = processToNextMin * 60f;

		return new GameTime((byte)hourWithProcess, (byte)minuteWithProcess, (byte)secondWithProcess, convertTo12HourTime: convertTo12HourTime);
	}
}
