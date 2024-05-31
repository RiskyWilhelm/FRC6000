using System;
using UnityEngine;

public sealed partial class ChickenAI : AIBase<ChickenAI.Statistics>
{
	// Initialize
	[Serializable]
	public sealed class Statistics : AIStatsBase, ICopyable<Statistics>
	{
		[SerializeField]
		private Timer _idleTimer = new(2f);

		public ref Timer IdleTimer
		{
			get => ref _idleTimer;
		}

		public void CopyTo(in Statistics main)
		{
			main.Velocity = this.Velocity;
			main.Power = this.Power;
			main.Health = this.Health;
			main.IdleTimer = this.IdleTimer;
		}
	}
	

	// Update
	protected override void DoIdle()
	{
		// Do idle when the timer finishes
		if (Stats.IdleTimer.Tick())
		{
			// Find a random horizontal position around self and set destination
			var randomHorizontalPosition = UnityEngine.Random.Range(-5, 6);
			var newDestination = selfRigidbody.position;

			newDestination.x += randomHorizontalPosition;
			SetDestinationTo(newDestination);
		}
	}

	protected override void OnStateChanged(AIState newState)
	{
		switch (newState)
		{
			case AIState.Dead:
			// TODO: Pool access
			//ChickenAIPool.Release(this);
			break;
		}

		base.OnStateChanged(newState);
	}

	public void OnRunAwayFromFox(Collider2D collider)
	{
		if (EventReflector.TryGetComponentByEventReflector<FoxAI>(collider.gameObject, out FoxAI foundChaserFox))
			SetDestinationToAwayFromFox(foundChaserFox);
	}

	public void SetDestinationToAwayFromFox(FoxAI fox)
	{
		// Get direction of: fox to the chicken
		// Add that direction to the position that exceeds destinationApproachThreshold
		var normalizedDirToChicken = (selfRigidbody.position - (Vector2)fox.transform.position).normalized;
		var newDestination = selfRigidbody.position;
		newDestination.x += normalizedDirToChicken.x * 1f;

		SetDestinationTo(newDestination);
	}

	public override void CopyTo(in AIBase main)
	{
		if (main is AIBase<ChickenAI.Statistics> aiBaseT)
			this.Stats.CopyTo(aiBaseT.Stats);
	}
}


#if UNITY_EDITOR

public sealed partial class ChickenAI
{ }

#endif