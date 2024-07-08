using UnityEngine;

public interface ITarget
{
	public TargetType TargetTag { get; }

	public uint Health { get; }

	public uint MaxHealth { get; }

	public bool IsDead => (Health == 0);


	public void TakeDamage(uint damage, Vector2 occuredWorldPosition);
}