using UnityEngine;

public sealed partial class BabyChickenAI : GroundedAIBase
{
	[Header("Movement")]
	#region

	[SerializeField, Range(0, 255)]
	private byte idleMaxDistance = 10;

	[SerializeField]
	private Timer idleTimer = new (2f);

	#endregion


	// Update
	protected override void DoIdle()
	{
		// Do idle when the timer finishes
		if (idleTimer.Tick())
		{
			// Find a random horizontal position around self and set destination
			var randomHorizontalPosition = UnityEngine.Random.Range(-idleMaxDistance, idleMaxDistance);
			var newDestination = selfRigidbody.position;
			newDestination.x += randomHorizontalPosition;

			SetDestinationTo(newDestination);
		}

		base.DoIdle();
	}

	protected override void OnStateChangedToDead()
	{
		ReleaseOrDestroySelf();
		base.OnStateChangedToDead();
	}

	public void OnRunawayTriggerStay2D(Collider2D collider)
	{
		// If there is any AI that is powerful than self nearby, run!
		if (EventReflector.TryGetComponentByEventReflector<AIBase>(collider.gameObject, out AIBase foundTarget)
			&& foundTarget.IsPowerfulThan(this))
		{
			SetDestinationToAwayFrom(foundTarget.transform.position);
		}
	}

	public override void CopyTo(in AIBase main)
	{
		if (main is BabyChickenAI babyChickenAI)
		{
			babyChickenAI.idleMaxDistance = this.idleMaxDistance;
			babyChickenAI.idleTimer = this.idleTimer;
		}

		base.CopyTo(main);
	}
}


#if UNITY_EDITOR

public sealed partial class BabyChickenAI
{ }

#endif