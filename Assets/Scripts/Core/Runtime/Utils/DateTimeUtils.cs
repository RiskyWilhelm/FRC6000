using System;

public static class DateTimeUtils
{
	/// <summary> By default, <see cref="DateTimeExtensions"/> and <see cref="DateTimeUtils"/> will offset the hour by this value to match Cardinal Direction. Means, degree of zero will equal to 18:00 in a circle </summary>
	public const float DefaultOffsetHour = 18f;

	public const float ExactHourDegree = (360f / 24f); // 1 hour = 15 degree

	public const float ExactMinutePoint = (ExactHourDegree / 60f); // 1 min = 0,25 degree

	public const float ExactSecondPoint = (ExactMinutePoint / 60f); // 1 second = 0,004166 degree


	// TODO: Find Year Month and Day from degree
	/// <remarks> Assumes the time 12:00 equals North(Y+ when facing to Z+) in cardinal direction. Means, degree of zero will equal to 18:00 in a circle </remarks>
	public static DateTime AngleDegreeToDateTime(float timeDegree, sbyte offsetHour = 0)
	{
		float offsetHourDegree = (offsetHour + DefaultOffsetHour) * ExactHourDegree;
		float verticallyMirroredDegree = (360f - timeDegree); // Angle is not inverted. Mirrored the angle in a circle vertically. That way, time will grow if rotation getting negative
		float calculatedDegree = (verticallyMirroredDegree + offsetHourDegree) % 360f;

		int hour = (int)(calculatedDegree / ExactHourDegree);
		calculatedDegree -= (hour * ExactHourDegree);

		int minute = (int)(calculatedDegree / ExactMinutePoint);
		calculatedDegree -= (minute * ExactMinutePoint);

		int second = (int)(calculatedDegree / ExactSecondPoint);
		return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);
	}
}
