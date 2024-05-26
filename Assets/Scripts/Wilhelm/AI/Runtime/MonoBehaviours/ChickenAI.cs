using UnityEngine;

public sealed partial class ChickenAI : AIBase
{
	public Timer idleTimer = new (2f);

	protected override void DoIdle()
	{
		// Check if reached the destination
		if (IsReachedToDestination() && idleTimer.Tick())
		{
			// Find a random horizontal position around self
			var randomHorizontalPosition = Random.Range(-5, 5);
			var newDestination = selfRigidbody.position;
			newDestination.x += randomHorizontalPosition;

			// Check if self able to go new horizontal position
			if (IsAbleToGo(newDestination))
			{
				SetDestinationTo(newDestination, 0.5f);
				return;
			}

			// If not, check if the other side is valid
			// If not, then no self has no luck
			newDestination.x += (-randomHorizontalPosition * 2);
			if (IsAbleToGo(newDestination))
				SetDestinationTo(newDestination, 0.5f);
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

		// Run if able to go
		if (IsAbleToGo(newDestination))
			SetDestinationTo(newDestination, 0.5f);
	}

	public void OnGotCaughtBy(AIBase chaser)
	{
		ChickenAIPool.Release(this);
	}
}


#if UNITY_EDITOR

public sealed partial class ChickenAI
{ }

#endif