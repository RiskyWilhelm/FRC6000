using System.Collections.Generic;
using System;

/// <summary> Takes input of <see cref="ICarryable"/> and any type of value </summary>
public struct ICarryableValue<T> : IEquatable<ICarryableValue<T>>
{
	public ICarryable carryable;

	public T value;


	public ICarryableValue(ICarryable carryable, T value)
	{
		this.carryable = carryable;
		this.value = value;
	}

	public override bool Equals(object obj)
	{
		return (obj is ICarryableValue<T> luck) && Equals(luck);
	}

	public bool Equals(ICarryableValue<T> other)
	{
		return carryable == other.carryable &&
			   EqualityComparer<T>.Default.Equals(value, other.value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(carryable, value);
	}

	public static bool operator ==(ICarryableValue<T> left, ICarryableValue<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ICarryableValue<T> left, ICarryableValue<T> right)
	{
		return !(left == right);
	}
}