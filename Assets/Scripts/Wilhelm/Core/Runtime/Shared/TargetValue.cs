using System.Collections.Generic;
using System;

/// <summary> Takes input of <see cref="ITarget"/> and any type of value </summary>
public struct TargetValue<T> : IEquatable<TargetValue<T>>
{
	public ITarget target;

	public T value;


	public TargetValue(ITarget target, T value)
	{
		this.target = target;
		this.value = value;
	}

	public override bool Equals(object obj)
	{
		return (obj is TargetValue<T> luck) && Equals(luck);
	}

	public bool Equals(TargetValue<T> other)
	{
		return target == other.target &&
			   EqualityComparer<T>.Default.Equals(value, other.value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(target, value);
	}

	public static bool operator ==(TargetValue<T> left, TargetValue<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TargetValue<T> left, TargetValue<T> right)
	{
		return !(left == right);
	}
}