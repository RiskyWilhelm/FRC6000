using UnityEngine;

public sealed partial class BabyChickenAI : AIBase<BabyChickenAIStats>
{
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
		if (EventReflector.TryGetComponentByEventReflector<BabyFoxAI>(collider.gameObject, out BabyFoxAI foundChaserFox))
			SetDestinationToAwayFromFox(foundChaserFox);
	}

	public void SetDestinationToAwayFromFox(BabyFoxAI fox)
	{
		// Get direction of: fox to the chicken
		// Add that direction to the position that exceeds destinationApproachThreshold
		var normalizedDirToChicken = (selfRigidbody.position - (Vector2)fox.transform.position).normalized;
		var newDestination = selfRigidbody.position;
		newDestination.x += normalizedDirToChicken.x * 1f;

		SetDestinationTo(newDestination);
	}
}


#if UNITY_EDITOR

public sealed partial class BabyChickenAI
{ }

#endif