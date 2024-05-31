using System;
using UnityEngine;

[Serializable]
public sealed partial class BabyChickenAIStats : AIStatsBase
{
	[SerializeField]
	private Timer _idleTimer = new(2f);

	public ref Timer IdleTimer
	{
		get => ref _idleTimer;
	}

	public override void CopyTo(in AIStatsBase main)
	{
		if (main is BabyChickenAIStats babyChickenAIStats)
			babyChickenAIStats.IdleTimer = this.IdleTimer;

		base.CopyTo(main);
	}
}

#if UNITY_EDITOR

public sealed partial class BabyChickenAIStats
{ }

#endif