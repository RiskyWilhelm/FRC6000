using System;
using System.Text;

public readonly struct GameTime : IEquatable<GameTime>
{
	public readonly byte hour;

	public readonly byte minute;

	public readonly byte second;

	public readonly bool IsNightTime => (hour >= 19);

	private static readonly StringBuilder timeBuilder = new();


	public override string ToString()
	{
		timeBuilder.Clear();
		return timeBuilder.AppendFormat("{0:00}:{1:00}:{2:00}", hour, minute, second).ToString();
	}

	public GameTime(byte hour, byte minute, byte second)
	{
		this.hour = hour;
		this.minute = minute;
		this.second = second;
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