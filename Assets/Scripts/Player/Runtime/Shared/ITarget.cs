using UnityEngine;

// TODO: Get rid of this ugly piece of interface implementation. Switch to Monobehaviour. Use relativeTo object like Interaction, Carry and EventReflector system does
public interface ITarget
{
	public TargetType TargetTag { get; }

	public uint Health { get; }

	public uint MaxHealth { get; }

	public bool IsDead => (Health == 0);


	public void TakeDamage(uint damage, Vector2 occuredWorldPosition);
}