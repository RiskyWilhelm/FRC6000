using System;
using System.Text;

public readonly struct GameTime : IEquatable<GameTime>
{
	public readonly byte hour;

	public readonly byte minute;

	public readonly byte second;

	public readonly DaylightType daylightType;

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
		this.minute = (byte)(minute % 60);
		this.second = (byte)(second % 60);
		isDayChangeHour = (hour == 0);

		// Check if it is night time or not
		if ((hour >= 19) || (hour <= 5))
			this.daylightType = DaylightType.Night;
		else
			this.daylightType = DaylightType.Light;

		// Convert if desired
		if (convertToPM)
			this.hour = (byte)(hour % 12);
		else
			this.hour = hour;
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