using System;
using UnityEngine;

[Serializable]
public sealed partial class ChickenAIStats : AIStats, ICopyable<ChickenAIStats>
{
	[SerializeField]
	private Timer _idleTimer = new (2f);

	public ref Timer IdleTimer
	{
		get => ref _idleTimer;
	}

	public void CopyTo(in ChickenAIStats main)
	{
		main.Velocity = this.Velocity;
		main.Power = this.Power;
		main.Health = this.Health;
		main.IdleTimer = this.IdleTimer;
	}
}


#if UNITY_EDITOR

public sealed partial class ChickenAIStats
{ }

#endif