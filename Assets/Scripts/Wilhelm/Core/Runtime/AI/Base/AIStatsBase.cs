using System;
using UnityEngine;

[Serializable]
public abstract partial class AIStatsBase : ICopyable<AIStatsBase>
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

	public virtual void Copy(in AIStatsBase other)
	{
		other.CopyTo(this);
	}

	public virtual void CopyTo(in AIStatsBase main)
	{
		main.Velocity = this.Velocity;
		main.Power = this.Power;
		main.Health = this.Health;
	}
}


#if UNITY_EDITOR

public abstract partial class AIStatsBase
{ }

#endif