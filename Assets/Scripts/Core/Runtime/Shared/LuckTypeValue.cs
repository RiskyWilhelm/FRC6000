using System.Collections.Generic;
using System;

/// <summary> Takes input of serializable <see cref="LuckType"/> and either serializable or non-serializable any type of value </summary>
[Serializable]
public struct LuckTypeValue<T> : IEquatable<LuckTypeValue<T>>
{
	public LuckType luckType;

	public T value;

	public override bool Equals(object obj)
	{
		return (obj is LuckTypeValue<T> luck) && Equals(luck);
	}

	public bool Equals(LuckTypeValue<T> other)
	{
		return luckType == other.luckType &&
			   EqualityComparer<T>.Default.Equals(value, other.value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(luckType, value);
	}

	public static bool operator ==(LuckTypeValue<T> left, LuckTypeValue<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(LuckTypeValue<T> left, LuckTypeValue<T> right)
	{
		return !(left == right);
	}
}