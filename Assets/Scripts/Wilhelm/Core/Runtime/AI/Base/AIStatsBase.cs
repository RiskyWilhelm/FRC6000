using System;
using UnityEngine;

[Serializable]
public abstract partial class AIStatsBase
{
	[field: SerializeField]
	public float Velocity { get; protected set; } = 5f;

	[field: SerializeField]
	public ushort Power { get; protected set; }

	[field: SerializeField]
	public ushort Health { get; protected set; }

	public bool IsDead => (Health == 0);


	// Update
	public void DecreaseHealth(ushort damage)
	{
		this.Health -= Math.Clamp(damage, ushort.MinValue, ushort.MaxValue);
	}
}


#if UNITY_EDITOR

public abstract partial class AIStatsBase
{ }

#endif