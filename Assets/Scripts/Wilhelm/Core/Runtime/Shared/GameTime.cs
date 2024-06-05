using System;
using System.Text;

public readonly struct GameTime : IEquatable<GameTime>
{
	public readonly byte hour;

	public readonly byte minute;

	public readonly byte second;

	public readonly DayLightType daylightType;

	public readonly bool isDayChangeHour;

	private static readonly StringBuilder timeBuilder = new();


	public override string ToString()
	{
		timeBuilder.Clear();
		return timeBuilder.AppendFormat("{0:00}:{1:00}:{2:00}", hour, minute, second).ToString();
	}

	/// <summary> Accepts 24-clock hour time </summary>
	/// <param name="convertToPM"> Converts to 12-hour clock </param>
	public GameTime(byte hour, byte minute, byte second, bool convertToPM = false)
	{
		// Check if it is night time or not
		if ((hour >= 19) || (hour <= 5))
			this.daylightType = DayLightType.Night;
		else
			this.daylightType = DayLightType.Light;

		// Check if hour is day change hour
		if (hour == 0)
			isDayChangeHour = true;
		else
			isDayChangeHour = false;

		// Convert if desired
		if (convertToPM)
			this.hour = (byte)(hour % 12);
		else
			this.hour = hour;

		this.minute = (byte)(minute % 60);
		this.second = (byte)(second % 60);
	}

	public override bool Equals(object obj)
	{
		return (obj is GameTime time) && Equals(time);
	}

	public bool Equals(GameTime other)
	{
		return hour == other.hour &&
			   minute == other.minute &&
			   second == other.second;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(hour, minute, second);
	}

	public static bool operator ==(GameTime left, GameTime right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GameTime left, GameTime right)
	{
		return !(left == right);
	}
}