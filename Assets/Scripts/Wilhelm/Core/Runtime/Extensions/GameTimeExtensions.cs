using System;

public static class GameTimeExtensions
{
	public static GameTime To12HourTime(this GameTime a)
	{
		if (!a.is12HourTime)
			return new GameTime(a, convertTo12HourTime: true);

		return a;
	}

	public static GameTime To24HourTime(this GameTime a)
	{
		if (a.is12HourTime)
		{
			switch (a.dayType)
			{
				case DayType.AM:
					return new GameTime(a);

				case DayType.PM:
					return new GameTime((byte)(a.hour + 12), a.minute, a.second);
			}
		}

		return a;
	}

	/// <summary> Assumes the time 12:00 equals North(Y+ when facing to Z+) in cardinal direction </summary>
	public static float ToAngle(this GameTime a, sbyte offsetHour = 0)
	{
		a = a.To24HourTime();

		float exactHourPoint = (360 / 24);
		float exactMinutePoint = (exactHourPoint / 60f);
		float exactSecondPoint = (exactMinutePoint / 60f);

		// offsetAngleDegree is used to shift the right side (angle zero) time to something else instead of 00:00
		float offsetAngleDegree = 270;
		float offsetHourAngle = (offsetHour * exactHourPoint) + offsetAngleDegree;

		float angleInDegrees = (a.hour * exactHourPoint);
		float verticallyMirroredAngleDegree = (360 - angleInDegrees); // Not inverted! Mirrored the angle in a circle. That way, time will grow if rotation getting negative

		float hourAngle = verticallyMirroredAngleDegree + offsetHourAngle;
		float minuteAngle = (a.minute * exactMinutePoint);
		float secondAngle = (a.second * exactSecondPoint);

		return Math.Abs(hourAngle - minuteAngle - secondAngle) % 360;
	}

	public static float ToProgress01(this GameTime a)
	{
		a = a.To24HourTime();

		float maxHour = 24f;
		float maxMinute = maxHour * 60f;
		float maxSecond = maxMinute * 60f;

		float completedMinutes = (a.hour * 60f) + a.minute;
		float completedSeconds = (completedMinutes * 60f) + a.second;

		float hourProgress = a.hour / maxHour;
		float minuteProgress = completedMinutes / maxMinute;
		float secondProgress = completedSeconds / maxSecond;

		return (hourProgress + minuteProgress + secondProgress) / 3;
	}
}
