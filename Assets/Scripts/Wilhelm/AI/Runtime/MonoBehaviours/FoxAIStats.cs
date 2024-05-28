using System;
using UnityEngine;

[Serializable]
public sealed partial class FoxAIStats : AIStats, ICopyable<FoxAIStats>
{
	[field: SerializeField]
	public ushort NormalAttackSpeed { get; private set; }

	// TODO: Encapsulate this field
	public bool IsCaughtChicken;

	public void CopyTo(in FoxAIStats main)
	{
		main.Velocity = this.Velocity;
		main.Power = this.Power;
		main.Health = this.Health;
		main.NormalAttackSpeed = this.NormalAttackSpeed;
	}
}


#if UNITY_EDITOR

public sealed partial class FoxAIStats
{ }

#endif