using System;
using System.Collections.Generic;

[Serializable]
public struct Priority<T> : IEquatable<Priority<T>>
{
	public int priority;

	public T value;

	public override bool Equals(object obj)
	{
		return (obj is Priority<T> priority) && Equals(priority);
	}

	public bool Equals(Priority<T> other)
	{
		return priority == other.priority &&
			   EqualityComparer<T>.Default.Equals(value, other.value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(priority, value);
	}

	public static bool operator ==(Priority<T> left, Priority<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Priority<T> left, Priority<T> right)
	{
		return !(left == right);
	}
}