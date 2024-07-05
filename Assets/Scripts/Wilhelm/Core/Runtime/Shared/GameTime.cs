using Cysharp.Text;
using System;

public readonly struct GameTime : IEquatable<GameTime>
{
	public readonly byte hour;

	public readonly byte minute;

	public readonly byte second;

	public readonly bool is12HourTime;

	public readonly DayType dayType;

	public readonly DaylightType daylightType;


	/// <summary> Accepts 24-hour time </summary>
	public GameTime(byte hour, byte minute, byte second, bool convertTo12HourTime = false)
	{
		this.hour = (byte)(hour % 24);
		this.minute = (byte)(minute % 60);
		this.second = (byte)(second % 60);
		this.is12HourTime = convertTo12HourTime;

		// Check day type
		if ((hour >= 0) && (hour <= 11))
			this.dayType = DayType.AM;
		else
			this.dayType = DayType.PM;

		// Check daylight type
		if ((hour >= 19) || (hour <= 5))
			this.daylightType = DaylightType.Night;
		else
			this.daylightType = DaylightType.Light;

		// Convert if desired
		if (convertTo12HourTime)
			this.hour = (byte)(hour % 12);
	}

	public GameTime(GameTime otherTime, bool convertTo12HourTime = false)
		: this(otherTime.hour, otherTime.minute, otherTime.second, convertTo12HourTime) { }

	public override string ToString()
	{
		return ZString.Format("{0:00}:{1:00}:{2:00} {3}", hour, minute, second, dayType);
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