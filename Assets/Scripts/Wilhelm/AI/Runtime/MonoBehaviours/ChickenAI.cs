using UnityEngine;

public sealed partial class ChickenAI : AIBase<ChickenAIStats>
{
	protected override void DoIdle()
	{
		// Do idle when the timer finishes
		if (Stats.IdleTimer.Tick())
		{
			// Find a random horizontal position around self and set destination
			var randomHorizontalPosition = Random.Range(-5, 5);
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
			ChickenAIPool.Release(this);
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
}


#if UNITY_EDITOR

public sealed partial class ChickenAI
{ }

#endif