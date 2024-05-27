using UnityEngine;

public sealed partial class ChickenAI : AIBase
{
	public Timer idleTimer = new (2f);

	protected override void DoIdle()
	{
		// Do idle when the timer finishes
		if (idleTimer.Tick())
		{
			// Find a random horizontal position around self and set destination
			var randomHorizontalPosition = Random.Range(-5, 5);
			var newDestination = selfRigidbody.position;

			newDestination.x += randomHorizontalPosition;
			SetDestinationTo(newDestination);
		}
	}

	public void RunAwayFromFox(Collider2D collider)
	{
		if (TryGetTargetFromCollider(collider, out FoxAI foundChaserFox))
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