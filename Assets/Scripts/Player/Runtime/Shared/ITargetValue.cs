using System.Collections.Generic;
using System;

/// <summary> Takes input of <see cref="ITarget"/> and any type of value </summary>
public struct ITargetValue<T> : IEquatable<ITargetValue<T>>
{
	public ITarget target;

	public T value;


	public ITargetValue(ITarget target, T value)
	{
		this.target = target;
		this.value = value;
	}

	public override bool Equals(object obj)
	{
		return (obj is ITargetValue<T> luck) && Equals(luck);
	}

	public bool Equals(ITargetValue<T> other)
	{
		return target == other.target &&
			   EqualityComparer<T>.Default.Equals(value, other.value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(target, value);
	}

	public static bool operator ==(ITargetValue<T> left, ITargetValue<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ITargetValue<T> left, ITargetValue<T> right)
	{
		return !(left == right);
	}
}