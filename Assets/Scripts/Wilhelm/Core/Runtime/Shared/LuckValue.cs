using System.Collections.Generic;
using System;

/// <summary> Takes input of serializable <see cref="LuckType"/> and either serializable or non-serializable any type of value </summary>
[Serializable]
public struct LuckValue<T> : IEquatable<LuckValue<T>>
{
	public LuckType luckType;

	public T value;

	public override bool Equals(object obj)
	{
		return (obj is LuckValue<T> luck) && Equals(luck);
	}

	public bool Equals(LuckValue<T> other)
	{
		return luckType == other.luckType &&
			   EqualityComparer<T>.Default.Equals(value, other.value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(luckType, value);
	}

	public static bool operator ==(LuckValue<T> left, LuckValue<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(LuckValue<T> left, LuckValue<T> right)
	{
		return !(left == right);
	}
}