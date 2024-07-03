using System.Collections.Generic;
using System;

/// <summary> Takes input of <see cref="IInteractable"/> and any type of value </summary>
public struct InteractableValue<T> : IEquatable<InteractableValue<T>>
{
	public IInteractable interactable;

	public T value;


	public InteractableValue(IInteractable target, T value)
	{
		this.interactable = target;
		this.value = value;
	}

	public override bool Equals(object obj)
	{
		return (obj is InteractableValue<T> luck) && Equals(luck);
	}

	public bool Equals(InteractableValue<T> other)
	{
		return interactable == other.interactable &&
			   EqualityComparer<T>.Default.Equals(value, other.value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(interactable, value);
	}

	public static bool operator ==(InteractableValue<T> left, InteractableValue<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(InteractableValue<T> left, InteractableValue<T> right)
	{
		return !(left == right);
	}
}