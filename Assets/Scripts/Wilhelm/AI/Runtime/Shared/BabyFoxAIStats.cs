using System;
using UnityEngine;

[Serializable]
public sealed partial class BabyFoxAIStats : AIStatsBase
{
	[field: SerializeField]
	public ushort NormalAttackSpeed { get; private set; }

	[NonSerialized]
	public bool IsCaughtChicken;

	public override void CopyTo(in AIStatsBase main)
	{
		if (main is BabyFoxAIStats babyFoxAIStats)
			babyFoxAIStats.NormalAttackSpeed = this.NormalAttackSpeed;

		base.CopyTo(main);
	}
}

#if UNITY_EDITOR

public sealed partial class BabyFoxAIStats
{ }

#endif