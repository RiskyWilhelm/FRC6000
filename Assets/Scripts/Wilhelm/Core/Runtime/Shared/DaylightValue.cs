using System.Collections.Generic;
using System;

/// <summary> Takes input of serializable <see cref="DaylightType"/> and either serializable or non-serializable any type of value </summary>
[Serializable]
public struct DaylightValue<T> : IEquatable<DaylightValue<T>>
{
	public DaylightType daylightType;

	public T value;

	public override bool Equals(object obj)
	{
		return (obj is DaylightValue<T> luck) && Equals(luck);
	}

	public bool Equals(DaylightValue<T> other)
	{
		return daylightType == other.daylightType &&
			   EqualityComparer<T>.Default.Equals(value, other.value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(daylightType, value);
	}

	public static bool operator ==(DaylightValue<T> left, DaylightValue<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(DaylightValue<T> left, DaylightValue<T> right)
	{
		return !(left == right);
	}
}