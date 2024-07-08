using System;

public static class DateTimeExtensions
{
	// TODO: Count in the Year, Month and Day
	/// <remarks> Assumes the time 12:00 equals North(Y+ when facing to Z+) in cardinal direction. Means, degree of zero will equal to 18:00 in a circle </remarks>
	public static float ToAngleDegree(this DateTime a, sbyte offsetHour = 0)
	{
		float offsetHourDegree = (offsetHour + DateTimeUtils.DefaultOffsetHour) * DateTimeUtils.ExactHourDegree;
		float verticallyMirroredHourDegree = 360f - (a.Hour * DateTimeUtils.ExactHourDegree); // Angle is not inverted. Mirrored the angle in a circle vertically. That way, time will grow if rotation getting negative

		float hourDegree = verticallyMirroredHourDegree + offsetHourDegree;
		float minuteDegree = a.Minute * DateTimeUtils.ExactMinutePoint;
        float secondDegree = a.Second * DateTimeUtils.ExactSecondPoint;

		return (hourDegree - minuteDegree - secondDegree) % 360f;
	}

	public static float ToProgress01(this DateTime a)
	{
		return 1f - (a.ToAngleDegree((sbyte)-DateTimeUtils.DefaultOffsetHour) / 360f);
	}
}
