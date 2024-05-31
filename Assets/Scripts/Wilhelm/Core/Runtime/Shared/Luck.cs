using System.Collections.Generic;
using System;

[Serializable]
public struct Luck<T> : IEquatable<Luck<T>>
{
	public LuckType luckType;

	public T value;

	public override bool Equals(object obj)
	{
		return (obj is Luck<T> luck) && Equals(luck);
	}

	public bool Equals(Luck<T> other)
	{
		return luckType == other.luckType &&
			   EqualityComparer<T>.Default.Equals(value, other.value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(luckType, value);
	}

	public static bool operator ==(Luck<T> left, Luck<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Luck<T> left, Luck<T> right)
	{
		return !(left == right);
	}
}