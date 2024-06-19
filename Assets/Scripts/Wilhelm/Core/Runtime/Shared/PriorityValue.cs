using System;
using System.Collections.Generic;

/// <summary> Takes input of serializable <see cref="DaylightType"/> and either serializable or non-serializable any type of value </summary>
[Serializable]
public struct PriorityValue<T> : IEquatable<PriorityValue<T>>
{
	public int priority;

	public T value;

	public override bool Equals(object obj)
	{
		return (obj is PriorityValue<T> priority) && Equals(priority);
	}

	public bool Equals(PriorityValue<T> other)
	{
		return priority == other.priority &&
			   EqualityComparer<T>.Default.Equals(value, other.value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(priority, value);
	}

	public static bool operator ==(PriorityValue<T> left, PriorityValue<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(PriorityValue<T> left, PriorityValue<T> right)
	{
		return !(left == right);
	}
}